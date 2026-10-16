using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Interface.Data.DashboardData
{
    public interface IDashboardData
    {
        Task<int> TotalStock();
        Task<IEnumerable<InventoryStatusTotalViewModel>> InventoryStatusOverView();
    }
}
