using ERP.Repository.ViewModel.Dashboard;
using ERP.Repository.ViewModel.Forecast;

namespace ERP.Repository.Interface.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> SalesOverView(DateTime from, DateTime to, CancellationToken cancellation);
        Task<InventoryOverViewModel> InventoryOverView();
        Task<IEnumerable<ForecastViewModel>> Forecasting();
    }
}
