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
        private const int WeeksPerYear = 52; // the chart lines up the same weeks in earlier years
        private const int SeasonWeeks = 13; // the chart shows 3 months either side of the forecast start

        // How many repeating shapes SSA keeps: the trend plus the yearly wave and its sharper
        // parts. Rank 5 tested best (docs/ssa-design/forecast-design.typ).
        private const int YearlyRank = 5;

        // SSA needs at least twice the window to train; the backtest hides 12 weeks on top.
        // Shorter histories use the average: an 8-week SSA lost to it in testing.
        private const int MinTrainWeeks = 2 * YearlyWindow; // 104
        private const int YearlyMinWeeks = MinTrainWeeks + BacktestWeeks; // 116

        // The chart's test panel: how many weeks to hide, and how far back the hidden weeks end
        private static readonly int[] HiddenWeekChoices = [4, 8, 12, 26, 52];
        private static readonly int[] EndWeeksAgoChoices = [0, 13, 26, 39]; // now, 3, 6 and 9 months ago

        // "Runs out around" further away than this isn't worth a date
        private const double MaxWeeksLeftForDate = 104;

        private static readonly TimeSpan MaxAge = TimeSpan.FromDays(1);

        // Dashboard widgets load in parallel; only one of them should run the forecast
        private static readonly SemaphoreSlim RunLock = new(1, 1);

        public async Task<ForecastPageViewModel> GetLatestForecast(
            int page,
            int pageSize,
            bool forceForecast,
            string? search,
            bool needOrderOnly
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            await EnsureFresh(forceForecast);

            var rows = await _forecast.GetForecastPage(page, pageSize, search, needOrderOnly);
            var count = await _forecast.ForecastCount(search, needOrderOnly);

            return new ForecastPageViewModel
            {
                ForecastResults = rows.Select(ToViewModel).ToList(),
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
                // The count and the accuracy describe the whole shop, not the filtered rows
                NeedOrderCount = await _forecast.NeedOrderCount(),
                Accuracy = await _forecast.Accuracy(),
                GeneratedAt = await _forecast.LatestForecastTime(),
            };
        }

        // The 13 weeks before the forecast, the forecast, and the same season in every earlier year
        public async Task<DemandChartViewModel> GetDemandChart(int productId)
        {
            await EnsureFresh(false);

            var result =
                await _forecast.GetProductForecast(productId)
                ?? throw new NotFound("No forecast for this product. It may be inactive.");

            var currentWeek = DemandWeek.StartOf(DemandWeek.PhToday());
            var entries = await _forecast.DemandEntries(productId);
            var historyStart = currentWeek.AddDays(-7 * result.HistoryWeeks);
            var shownWeeks = Math.Min(SeasonWeeks, result.HistoryWeeks);
            var firstWeek = currentWeek.AddDays(-7 * shownWeeks);
            var history = WeeklySeries(firstWeek, currentWeek, entries);

            // Step back one year (52 weeks, so weeks keep starting on the same weekday)
            // while the season window still overlaps the product's history
            var pastYears = new List<DemandPastYearViewModel>();
            for (var year = 1; ; year++)
            {
                var start = currentWeek.AddDays(-7 * WeeksPerYear * year);
                var windowStart = start.AddDays(-7 * SeasonWeeks);
                var windowEnd = start.AddDays(7 * SeasonWeeks);
                if (windowEnd <= historyStart)
                {
                    break;
                }

                var weeks = WeeklySeries(windowStart, windowEnd, entries)
                    .Select((demand, i) => windowStart.AddDays(7 * i) < historyStart ? (double?)null : demand)
                    .ToList();
                var sameWeeks = weeks.Skip(SeasonWeeks).Take(Horizon).ToList();

                pastYears.Add(
                    new DemandPastYearViewModel
                    {
                        Year = start.Year,
                        Weeks = weeks,
                        SameWeeksTotal = sameWeeks.Any(w => w == null) ? null : sameWeeks.Sum(),
                    }
                );
            }

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
                PastYears = pastYears,
            };
        }

        // Hide `hiddenWeeks` weeks ending `endWeeksAgo` weeks before now, predict them from the weeks
        // before, and compare with what sold. Recalculated on every request; nothing is stored.
        public async Task<DemandBacktestViewModel> GetBacktest(int productId, int hiddenWeeks, int endWeeksAgo)
        {
            if (!HiddenWeekChoices.Contains(hiddenWeeks))
            {
                throw new BadRequest($"Hidden weeks must be one of {string.Join(", ", HiddenWeekChoices)}.");
            }

            if (!EndWeeksAgoChoices.Contains(endWeeksAgo))
            {
                throw new BadRequest($"The test must end one of {string.Join(", ", EndWeeksAgoChoices)} weeks ago.");
            }

            var product =
                (await _forecast.ForecastProducts()).FirstOrDefault(p => p.ProductId == productId)
                ?? throw new NotFound("No forecast for this product. It may be inactive.");

            var currentWeek = DemandWeek.StartOf(DemandWeek.PhToday());
            var entries = await _forecast.DemandEntries(productId);
            var series = WeeklySeries(FirstWeek(product, entries, currentWeek), currentWeek, entries);

            var view = new DemandBacktestViewModel { HiddenWeeks = hiddenWeeks, EndWeeksAgo = endWeeksAgo };
            var trainWeeks = series.Count - endWeeksAgo - hiddenWeeks;
            if (trainWeeks < MinTrainWeeks)
            {
                return view;
            }

            var test = Backtest(series, hiddenWeeks, endWeeksAgo);
            if (test.Ssa == null)
            {
                return view;
            }

            var hiddenStart = currentWeek.AddDays(-7 * (endWeeksAgo + hiddenWeeks));
            var before = test.Train.TakeLast(SeasonWeeks).ToList();

            view.Testable = true;
            view.TrainWeeks = trainWeeks;
            view.Before = before
                .Select(
                    (demand, i) =>
                        new DemandHistoryPointViewModel
                        {
                            WeekStart = hiddenStart.AddDays(-7 * (before.Count - i)),
                            Demand = demand,
                        }
                )
                .ToList();
            view.Weeks = test
                .Hidden.Select(
                    (actual, i) =>
                        new DemandBacktestWeekViewModel
                        {
                            WeekStart = hiddenStart.AddDays(7 * i),
                            Actual = actual,
                            Low = Math.Max(0, test.Ssa.Low[i]),
                            Expected = Math.Max(0, test.Ssa.Expected[i]),
                            BusyCase = Math.Max(0, test.Ssa.BusyCase[i]),
                            Baseline = test.Baseline,
                        }
                )
                .ToList();
            view.SsaErrorPercent = test.Sold > 0 ? test.SsaError / test.Sold * 100 : null;
            view.BaselineErrorPercent = test.Sold > 0 ? test.BaselineError / test.Sold * 100 : null;

            return view;
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

        // Pick the method by how much history there is, backtest SSA, and predict the next 4 weeks
        private static ForecastResult Forecast(List<double> series)
        {
            if (series.Count < YearlyMinWeeks)
            {
                return AverageForecast(series, ForecastMethodEnum.Average);
            }

            // Backtest: hide the last 12 weeks, predict them, compare with the simple guess
            var test = Backtest(series, BacktestWeeks, 0);
            var forecast = Ssa(series, Horizon);

            // The backtest is shown, not used to switch methods: picking the average whenever
            // SSA lost one product's last 12 weeks made the forecast worse overall in testing.
            // The average is only used when SSA can't model this history at all.
            var result =
                forecast == null ? AverageForecast(series, ForecastMethodEnum.AverageFallback) : SsaForecast(forecast);

            result.BacktestSold = test.Sold;
            result.SsaAbsError = test.SsaError;
            result.BaselineAbsError = test.BaselineError;

            return result;
        }

        private record BacktestRun(
            List<double> Train,
            List<double> Hidden,
            SsaForecast? Ssa,
            double Baseline,
            double Sold,
            double? SsaError,
            double BaselineError
        );

        // Drop the last `endWeeksAgo` weeks, hide the `hiddenWeeks` before them, and predict the hidden
        // weeks from everything earlier. Errors are summed (WAPE = error / sold); SSA's guesses can't go below 0.
        private static BacktestRun Backtest(List<double> series, int hiddenWeeks, int endWeeksAgo)
        {
            var end = series.Count - endWeeksAgo;
            var train = series.Take(end - hiddenWeeks).ToList();
            var hidden = series.Skip(end - hiddenWeeks).Take(hiddenWeeks).ToList();
            var ssa = Ssa(train, hiddenWeeks);
            var baseline = Average(train);

            return new BacktestRun(
                train,
                hidden,
                ssa,
                baseline,
                hidden.Sum(),
                ssa == null
                    ? null
                    : hidden.Select((actual, i) => Math.Abs(actual - Math.Max(0, ssa.Expected[i]))).Sum(),
                hidden.Select(actual => Math.Abs(actual - baseline)).Sum()
            );
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

        // Plain arithmetic, no forecasting model: order enough for the busy case
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
                SsaErrorPercent = f.BacktestSold > 0 ? f.SsaAbsError / f.BacktestSold * 100 : null,
                BaselineErrorPercent = f.BacktestSold > 0 ? f.BaselineAbsError / f.BacktestSold * 100 : null,
            };
        }
    }
}
