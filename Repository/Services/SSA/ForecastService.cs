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

            var series = data.Select(x => new DemandData { NetChange = (float)x.NetChange });
            var view = mLContext.Data.LoadFromEnumerable(series);

            var Engine = mLContext.Forecasting.ForecastBySsa(
                outputColumnName: "Forecast",
                inputColumnName: "NetChange",
                windowSize: 30,
                seriesLength: series.Count(),
                trainSize: series.Count(),
                horizon: 30,
                confidenceLevel: 95
                );

            var model = Engine.Fit(view);

            var foreCastEngine = model.CreateTimeSeriesEngine<DemandData, ForeCastResultViewModel>(mLContext);
            var forecast = foreCastEngine.Predict();

            float currentStock = data.Last().EndDayStock;
            var lastDay = data.Last().Day ?? DateTime.Today;

            var stockOuts = forecast.Forecast.Select((predicted, index) => {


                currentStock += predicted;
                return new FinalForecastViewModel
                {
                    InventoryId = data.Last().InventoryId,
                    Day = lastDay.AddDays(index + 1),
                    Stock = currentStock,
                };
            })
            .Where(x => x.Stock <= 0)
            .ToList();

            return stockOuts;
        }
    }
}
