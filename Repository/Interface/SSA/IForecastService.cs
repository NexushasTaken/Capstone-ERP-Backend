using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task<ForecastPageViewModel> GetLatestForecast(int page, int pageSize, bool forceForecast);
        Task<DemandChartViewModel> GetDemandChart(int productId);
    }
}
