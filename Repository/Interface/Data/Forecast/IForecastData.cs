using ERP.Repository.Model.Forecast;
using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.Data.Forecast
{
    public interface IForecastData : IBaseData
    {
        Task<List<DemandEntryViewModel>> DemandEntries(int? productId = null);
        Task<List<ForecastProductViewModel>> ForecastProducts();
        Task<DateTime?> LatestForecastTime();
        Task ReplaceForecast(IEnumerable<ForecastResult> results);
        Task<List<ForecastResult>> GetForecastPage(int page, int pageSize, string? search, bool needOrderOnly);
        Task<int> ForecastCount(string? search, bool needOrderOnly);
        Task<int> NeedOrderCount();
        Task<ForecastAccuracyViewModel?> Accuracy();
        Task<ForecastResult?> GetProductForecast(int productId);
    }
}
