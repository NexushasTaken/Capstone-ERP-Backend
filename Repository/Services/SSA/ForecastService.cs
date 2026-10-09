using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.Forecast;
using ERP.Repository.Interface.SSA;
using ERP.Repository.Model.Forecast;
using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Services.SSA
{
    // Demand forecast per product (docs/ssa-design/forecast-design.typ):
    // weekly demand -> SSA (or a simple average) -> next 4 weeks with a 95% range
    // -> suggested order, plus a backtest on the last 12 weeks against a simple average.
    public class ForecastService(IForecastData _forecast) : IForecastService
    {
        private const int YearlyWindow = 52; // look for a yearly pattern
        private const int Horizon = 4; // weeks to predict, a normal restock cycle
        private const int BacktestWeeks = 12; // weeks hidden from the model in the backtest
        private const int BaselineWeeks = 4; // the simple guess averages this many weeks
        private const int ChartWeeks = 26; // demand history shown on the chart

        // How many repeating shapes SSA keeps: the trend plus the yearly wave and its sharper
        // parts. Rank 5 tested best (docs/ssa-design/forecast-design.typ).
        private const int YearlyRank = 5;

        // SSA needs at least twice the window to train; the backtest hides 12 weeks on top.
        // Shorter histories use the average: an 8-week SSA lost to it in testing.
        private const int YearlyMinWeeks = 2 * YearlyWindow + BacktestWeeks; // 116

        // "Runs out around" further away than this isn't worth a date
        private const double MaxWeeksLeftForDate = 104;

        private static readonly TimeSpan MaxAge = TimeSpan.FromDays(1);

        // Dashboard widgets load in parallel; only one of them should run the forecast
        private static readonly SemaphoreSlim RunLock = new(1, 1);

        public async Task<ForecastPageViewModel> GetLatestForecast(int page, int pageSize, bool forceForecast)
        {
            PageQueryValidator.Ensure(page, pageSize);

            await EnsureFresh(forceForecast);

            var rows = await _forecast.GetForecastPage(page, pageSize);
            var count = await _forecast.ForecastCount();

            return new ForecastPageViewModel
            {
                ForecastResults = rows.Select(ToViewModel).ToList(),
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
                NeedOrderCount = await _forecast.NeedOrderCount(),
                Accuracy = await _forecast.Accuracy(),
                GeneratedAt = await _forecast.LatestForecastTime(),
            };
        }

        public async Task<DemandChartViewModel> GetDemandChart(int productId)
        {
            await EnsureFresh(false);

            var result =
                await _forecast.GetProductForecast(productId)
                ?? throw new NotFound("No forecast for this product. It may be inactive.");

            var currentWeek = DemandWeek.StartOf(DemandWeek.PhToday());
            var entries = await _forecast.DemandEntries(productId);
            var firstWeek = currentWeek.AddDays(-7 * Math.Min(ChartWeeks, result.HistoryWeeks));
            var history = WeeklySeries(firstWeek, currentWeek, entries);

            return new DemandChartViewModel
            {
                Product = ToViewModel(result),
                History = history
                    .Select(
                        (demand, i) =>
                            new DemandHistoryPointViewModel { WeekStart = firstWeek.AddDays(7 * i), Demand = demand }
                    )
                    .ToList(),
                Forecast = result
                    .Weeks.Select(w => new DemandForecastPointViewModel
                    {
                        WeekStart = w.WeekStart,
                        Low = w.Low,
                        Expected = w.Expected,
                        BusyCase = w.BusyCase,
                    })
                    .ToList(),
            };
        }

        // Re-run when forced, when there is no forecast yet, or when the latest one is over a day old
        private async Task EnsureFresh(bool force)
        {
            if (!force && !await IsStale())
            {
                return;
            }

            await RunLock.WaitAsync();
            try
            {
                if (force || await IsStale())
                {
                    await RunForecast();
                }
            }
            finally
            {
                RunLock.Release();
            }
        }

        private async Task<bool> IsStale()
        {
            var latest = await _forecast.LatestForecastTime();
            return latest == null || latest < DateTime.UtcNow - MaxAge;
        }

        private async Task RunForecast()
        {
            var products = await _forecast.ForecastProducts();
            var entriesByProduct = (await _forecast.DemandEntries()).ToLookup(e => e.ProductId);

            var today = DemandWeek.PhToday();
            var currentWeek = DemandWeek.StartOf(today);
            var now = DateTime.UtcNow;

            var results = new List<ForecastResult>();

            foreach (var product in products)
            {
                var entries = entriesByProduct[product.ProductId].ToList();
                var firstWeek = FirstWeek(product, entries, currentWeek);
                var series = WeeklySeries(firstWeek, currentWeek, entries);

                var result = Forecast(series);
                result.ProductId = product.ProductId;
                result.HistoryWeeks = series.Count;
                result.StockOnHand = product.StockOnHand;
                result.Created_At = now;
                result.IsActive = true;

                foreach (var (week, i) in result.Weeks.Select((w, i) => (w, i)))
                {
                    week.WeekStart = currentWeek.AddDays(7 * i);
                    week.Created_At = now;
                    week.IsActive = true;
                }

                Recommend(result, today);
                results.Add(result);
            }

            await _forecast.ReplaceForecast(results);
        }

        // Pick the method by how much history there is, backtest the AI, and predict the next 4 weeks
        private static ForecastResult Forecast(List<double> series)
        {
            if (series.Count < YearlyMinWeeks)
            {
                return AverageForecast(series, ForecastMethodEnum.Average);
            }

            // Backtest: hide the last 12 weeks, predict them, compare with the simple guess
            var train = series.Take(series.Count - BacktestWeeks).ToList();
            var hidden = series.Skip(series.Count - BacktestWeeks).ToList();
            var aiGuess = Ssa(train, BacktestWeeks);
            var forecast = Ssa(series, Horizon);
            var baselineGuess = Average(train);

            var sold = hidden.Sum();
            var baselineError = hidden.Select(actual => Math.Abs(actual - baselineGuess)).Sum();
            double? aiError =
                aiGuess == null
                    ? null
                    : hidden.Select((actual, i) => Math.Abs(actual - Math.Max(0, aiGuess.Expected[i]))).Sum();

            // The backtest is shown, not used to switch methods: picking the average whenever
            // SSA lost one product's last 12 weeks made the forecast worse overall in testing.
            // The average is only used when SSA can't model this history at all.
            var result =
                forecast == null ? AverageForecast(series, ForecastMethodEnum.AverageFallback) : SsaForecast(forecast);

            result.BacktestSold = sold;
            result.AiAbsError = aiError;
            result.BaselineAbsError = baselineError;

            return result;
        }

        private static ForecastResult SsaForecast(SsaForecast forecast)
        {
            return WithWeeks(
                ForecastMethodEnum.YearlySsa,
                Enumerable
                    .Range(0, Horizon)
                    .Select(i => new ForecastWeek
                    {
                        Low = forecast.Low[i],
                        Expected = forecast.Expected[i],
                        BusyCase = forecast.BusyCase[i],
                    })
            );
        }

        // "Next weeks sell the average of the last 4 weeks". It has no range, so low = busy case = expected.
        private static ForecastResult AverageForecast(List<double> series, ForecastMethodEnum method)
        {
            var average = Average(series);

            return WithWeeks(
                method,
                Enumerable
                    .Range(0, Horizon)
                    .Select(_ => new ForecastWeek
                    {
                        Low = average,
                        Expected = average,
                        BusyCase = average,
                    })
            );
        }

        // You can't sell fewer than zero, so negative weeks become 0 before adding up 4-week totals
        private static ForecastResult WithWeeks(ForecastMethodEnum method, IEnumerable<ForecastWeek> weeks)
        {
            var list = weeks
                .Select(w => new ForecastWeek
                {
                    Low = Math.Max(0, w.Low),
                    Expected = Math.Max(0, w.Expected),
                    BusyCase = Math.Max(0, w.BusyCase),
                })
                .ToList();

            return new ForecastResult
            {
                Method = (int)method,
                Weeks = list,
                LowDemand = list.Sum(w => w.Low),
                ExpectedDemand = list.Sum(w => w.Expected),
                BusyDemand = list.Sum(w => w.BusyCase),
            };
        }

        // Plain arithmetic, no AI: order enough for the busy case
        private static void Recommend(ForecastResult result, DateOnly today)
        {
            result.SuggestedOrder = Math.Max(0, (int)Math.Ceiling(result.BusyDemand - result.StockOnHand));

            var weeklyExpected = result.ExpectedDemand / Horizon;
            if (weeklyExpected <= 0.01)
            {
                return; // not expected to sell, so it doesn't run out
            }

            result.WeeksLeft = result.StockOnHand / weeklyExpected;
            if (result.WeeksLeft <= MaxWeeksLeftForDate)
            {
                result.RunsOutAround = today.AddDays((int)Math.Floor(result.WeeksLeft.Value * 7));
            }
        }

        private static SsaForecast? Ssa(List<double> series, int horizon) =>
            SsaForecaster.Forecast(series, YearlyWindow, YearlyRank, horizon);

        private static double Average(List<double> series)
        {
            var last = series.TakeLast(BaselineWeeks).ToList();
            return last.Count == 0 ? 0 : last.Average();
        }

        // The week the product was first stocked or first sold, whichever is earlier
        private static DateOnly FirstWeek(
            ForecastProductViewModel product,
            List<DemandEntryViewModel> entries,
            DateOnly currentWeek
        )
        {
            var first = currentWeek;

            if (product.FirstStocked != null)
            {
                first = Min(first, DemandWeek.StartOf(product.FirstStocked.Value));
            }

            if (entries.Count > 0)
            {
                first = Min(first, DemandWeek.StartOf(entries.Min(e => e.CreatedAt)));
            }

            return first;
        }

        private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

        // Demand per complete week from firstWeek up to the week before currentWeek.
        // Weeks without sales count as 0; a week with more returns than sales counts as 0.
        private static List<double> WeeklySeries(
            DateOnly firstWeek,
            DateOnly currentWeek,
            IEnumerable<DemandEntryViewModel> entries
        )
        {
            var weeks = Math.Max(0, (currentWeek.DayNumber - firstWeek.DayNumber) / 7);
            var series = new double[weeks];

            foreach (var entry in entries)
            {
                var index = (DemandWeek.StartOf(entry.CreatedAt).DayNumber - firstWeek.DayNumber) / 7;
                if (index >= 0 && index < weeks)
                {
                    series[index] += entry.Units;
                }
            }

            return series.Select(units => Math.Max(0, units)).ToList();
        }

        private static DemandForecastViewModel ToViewModel(ForecastResult f)
        {
            return new DemandForecastViewModel
            {
                ProductId = f.ProductId,
                Name = f.Product?.Name,
                LowDemand = f.LowDemand,
                ExpectedDemand = f.ExpectedDemand,
                BusyDemand = f.BusyDemand,
                StockOnHand = f.StockOnHand,
                WeeksLeft = f.WeeksLeft,
                RunsOutAround = f.RunsOutAround,
                SuggestedOrder = f.SuggestedOrder,
                Method = f.Method,
                HistoryWeeks = f.HistoryWeeks,
                AiErrorPercent = f.BacktestSold > 0 ? f.AiAbsError / f.BacktestSold * 100 : null,
                BaselineErrorPercent = f.BacktestSold > 0 ? f.BaselineAbsError / f.BacktestSold * 100 : null,
            };
        }
    }
}
