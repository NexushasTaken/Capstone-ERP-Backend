using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task SsaModel(int page, int pageSize, bool dataModeling);
        Task<ForecastPageViewModel> GetLatestForecast(int page, int pageSize, bool forceForecast);
    }
}
