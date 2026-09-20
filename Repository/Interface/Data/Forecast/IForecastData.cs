using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.Data.Forecast
{
    public interface IForecastData : IBaseData
    {
        Task<IEnumerable<ForecastViewModel>> Movement();
        Task<ForecastResult> GetSingleLatestForecast();
        Task<IEnumerable<ForecastResult>> GetThirtyDaysForecast(int page, int pageSize);
        Task<int> ForecastResultTotalCount();
        Task TruncateForecastTable();
    }
}
