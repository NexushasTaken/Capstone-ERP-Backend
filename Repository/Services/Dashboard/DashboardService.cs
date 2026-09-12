using ERP.Repository.Interface.Dashboard;
using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.ViewModel.Dashboard;

namespace ERP.Repository.Services.Dashboard
{
    public class DashboardService(IDashboardData _dashboard) : IDashboardService
    {
        public async Task<DashboardViewModel> SalesOverView(DateTime from, DateTime to, CancellationToken cancellation)
        {

            var data = await _dashboard.OverView(from,to,cancellation);

            var prevMonths = await _dashboard.PrevMonths(from, to, cancellation);

            var latest = data.Sum(x => x.Data);
            var prev = prevMonths.Sum(x => x.Data);

            var growth = prev == 0 ? 0 : ((latest - prev) / prev) * 100;


            var result = new DashboardViewModel
            {
                Data = data,
                TotalSales = latest,
                GrowthPercentage = (double)growth
            };

            return result;
        }

        public async Task<InventoryOverViewModel> InventoryOverView()
        {
            var warehouse = await _dashboard.WarehouseTotal();

            var inventoryStatus = await _dashboard.InventoryStatusOverView();

            throw new NotImplementedException();
        }
    }
}
