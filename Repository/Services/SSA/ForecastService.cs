using ERP.Repository.Interface.Data.Forecast;
using ERP.Repository.Interface.SSA;
using ERP.Repository.Model.Forecast;
using ERP.Repository.ViewModel.Forecast;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace ERP.Repository.Services.SSA
{
    public class ForecastService(IForecastData _forecast, MLContext mLContext) : IForecastService
    {
        
        public async Task<ForecastPageViewModel> GetLatestForecast(int page, int pageSize, bool forceForecast)
        {
            var lastForecast = await _forecast.GetSingleLatestForecast();

            if (lastForecast == null || lastForecast.Created_At > DateTime.UtcNow)
            {
                await SsaModel(page, pageSize, true);
            }

            if (forceForecast)
            {
                await SsaModel(page, pageSize, true);
            }

            var data = await _forecast.GetThirtyDaysForecast(page, pageSize);

            var count = await _forecast.ForecastResultTotalCount();

            var final =  data.Select(f => new FinalForecastViewModel
            {
                InventoryId  = f.InventoryId,
                Name = f.Inventory.Name,
                EarliestStockOutDay = f.EarliestStockOutDay
            }).ToList();

            return new ForecastPageViewModel
            {
                ForecastResults = final,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count
            };
        }


        public async Task SsaModel(int page, int pageSize, bool dataModeling) 
        {
            var data = await _forecast.Movement();

            data = FillDaysGap(data);
            
            var result = new List<FinalForecastViewModel>();
            DateTime today = DateTime.Today;

            const int windowSize = 7;

            foreach (var group in data.GroupBy(x => x.InventoryId))
            {
                var series = group.Select(x => new DemandData { EndDayStock = x.EndDayStock });

                if(series.Count() <= 2 * windowSize)
                {
                    continue;
                }

                var view = mLContext.Data.LoadFromEnumerable(series);

                var engine = mLContext.Forecasting.ForecastBySsa(
                outputColumnName: "Forecast",
                inputColumnName: "EndDayStock",
                windowSize: windowSize,
                seriesLength: Math.Min(series.Count(), 60),
                trainSize: Math.Min(series.Count(), 365),
                horizon: 30,
                confidenceLevel: 0.95f
                );

                var model = engine.Fit(view);

                var forecastEngine = model.CreateTimeSeriesEngine<DemandData, ForeCastResultViewModel>(mLContext);

                var forecast = forecastEngine.Predict();

                var lastDay = group.Max(x => x.Day) ?? DateTime.Today;

                var stockTrajectory = forecast.Forecast.Select((predicted, index) =>
                {
                    return new { Day = lastDay.AddDays(index + 1), Stock = predicted };
                })
                  .Where(x => x.Day >= today)
                  .ToList();

                var earliest = stockTrajectory.FirstOrDefault(x => x.Stock <= 0);

                int zeroDays = stockTrajectory.Count(x => x.Stock == 0);
                double probNext30Days = (double)zeroDays / stockTrajectory.Count() * 100;

                if (earliest != null)
                {
                    result.Add(new FinalForecastViewModel
                    {
                        InventoryId = group.Key,
                        EarliestStockOutDay = earliest?.Day,
                        //ProbabilityNext30Days = probNext30Days,
                        //Stock = stockTrajectory.Last().Stock
                    });
                }
            }

            var existing = await _forecast.GetThirtyDaysForecast(page, pageSize, dataModeling);

            if (!existing.Any())
            {
                var forecastResults = result.Select(f => new ForecastResult
                {
                    InventoryId = f.InventoryId,
                    EarliestStockOutDay = f.EarliestStockOutDay,
                    Created_At = DateTime.UtcNow,
                    IsActive = true
                }).OrderBy(f => f.EarliestStockOutDay).ToList();

                await _forecast.SaveMany(forecastResults);

                return;
            }

            var forecastResult = result
                .Where(f => !existing.Any(e => e.InventoryId == f.InventoryId && e.EarliestStockOutDay == f.EarliestStockOutDay))
                .Select(f => new ForecastResult
                {
                    InventoryId = f.InventoryId,
                    EarliestStockOutDay = f.EarliestStockOutDay,
                    Created_At = DateTime.UtcNow,
                    IsActive = true

                }).OrderBy(f => f.EarliestStockOutDay).ToList();

            await _forecast.SaveMany(forecastResult);

            return;
        }

        private IEnumerable<ForecastViewModel> FillDaysGap(IEnumerable<ForecastViewModel> data)
        {
            return data
                .GroupBy(x => x.InventoryId)
                .SelectMany(group =>
                {
                    var minDate = group.Min(x => x.Day);
                    var maxDate = group.Max(x => x.Day);

                    var allDays = Enumerable.Range(0, (maxDate - minDate).Value.Days + 1)
                    .Select(offset => minDate.Value.AddDays(offset));

                    int lastKnownStock = group.First().EndDayStock;

                    return allDays.Select(day =>
                    {
                        var existing = group.FirstOrDefault(x => x.Day == day);
                        if(existing != null)
                        {
                            lastKnownStock = existing.EndDayStock;
                            return existing;
                        }

                        return new ForecastViewModel
                        {
                            InventoryId = group.Key,
                            Day = day,
                            NetChange = 0,
                            EndDayStock = lastKnownStock
                        };
                    });
                });
        }
    }
}
