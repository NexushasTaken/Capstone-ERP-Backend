using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task<List<FinalForecastViewModel>> SsaModel();
        Task<IEnumerable<FinalForecastViewModel>> GetLatestForecast(bool forceForecast);
    }
}
