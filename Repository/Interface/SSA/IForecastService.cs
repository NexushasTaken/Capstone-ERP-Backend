using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task<ForecastPageViewModel> GetLatestForecast(
            int page,
            int pageSize,
            bool forceForecast,
            string? search,
            bool needOrderOnly
        );
        Task<DemandChartViewModel> GetDemandChart(int productId);
        Task<DemandBacktestViewModel> GetBacktest(int productId, int hiddenWeeks, int endWeeksAgo);
    }
}
