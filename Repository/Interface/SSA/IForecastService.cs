using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.SSA
{
    public interface IForecastService
    {
        Task<List<FinalForecastViewModel>> SsaModel();
    }
}
