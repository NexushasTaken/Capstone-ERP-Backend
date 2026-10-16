using ERP.Repository.Interface.Dashboard;
using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Services.Dashboard
{
    public class DashboardService(IDashboardData _dashboard) : IDashboardService
    {
        public async Task<InventoryOverViewModel> InventoryOverView()
        {
            var totalStock = await _dashboard.TotalStock();

            var inventoryStatus = await _dashboard.InventoryStatusOverView();

            return new InventoryOverViewModel
            {
                TotalStock = totalStock,
                Risk = 0,
                InventoryStatus = inventoryStatus,
            };
        }
    }
}
