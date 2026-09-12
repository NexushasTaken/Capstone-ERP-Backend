using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Interface.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> SalesOverView(DateTime from, DateTime to, CancellationToken cancellation);
    }
}
