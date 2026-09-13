using ERP.Repository.Interface.Data.Forecast;
using ERP.Repository.Interface.SSA;
using ERP.Repository.ViewModel.Forecast;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace ERP.Repository.Services.SSA
{
    public class ForecastService(IForecastData _forecast, MLContext mLContext) : IForecastService
    {
        public async Task<List<FinalForecastViewModel>> SsaModel() 
        {
            var data = await _forecast.Movement();


            var result = new List<FinalForecastViewModel>();

            foreach (var group in data.GroupBy(x => x.InventoryId))
            {
                var series = group.Select(x => new DemandData { NetChange = x.NetChange });

                var view = mLContext.Data.LoadFromEnumerable(series);

                var engine = mLContext.Forecasting.ForecastBySsa(
                outputColumnName: "Forecast",
                inputColumnName: "NetChange",
                windowSize: 7,
                seriesLength: 60,
                trainSize: 365,
                horizon: 30,
                confidenceLevel: 0.95f
                );

                var model = engine.Fit(view);

                var forecastEngine = model.CreateTimeSeriesEngine<DemandData, ForeCastResultViewModel>(mLContext);

                var forecast = forecastEngine.Predict();

                float currentStock = group.Last().EndDayStock;

                var lastDay = group.Last().Day ?? DateTime.Today;

                var stockOuts = forecast.Forecast.Select((predicted, index) => new FinalForecastViewModel
                {
                    InventoryId = group.Key,
                    Day = lastDay.AddDays(index + 1),
                    Stock = currentStock += predicted
                })
                .Where(x => x.Stock <= 0)
                .ToList();

                result.AddRange(stockOuts);
            }

            return result;
        }
    }
}
