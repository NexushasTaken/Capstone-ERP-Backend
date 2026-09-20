using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task SsaModel();
        Task<ForecastPageViewModel> GetLatestForecast(int page, int pageSize, bool forceForecast);
    }
}
