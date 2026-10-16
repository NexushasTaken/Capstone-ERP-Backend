using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Interface.Dashboard
{
    public interface IDashboardService
    {
        Task<InventoryOverViewModel> InventoryOverView();
    }
}
