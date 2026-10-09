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
            var data = await _dashboard.OverView(from, to, cancellation);

            var prevMonths = await _dashboard.PrevMonths(from, to, cancellation);

            var latest = data.Sum(x => x.Data);
            decimal prev = prevMonths.Sum(x => x.Data);

            decimal growth = 0;
            string error = "";

            if (prev == 0 || prevMonths.Count() < data.Count())
            {
                error = "Insufficient previous month data for growth calculation";
            }
            else
            {
                growth = ((latest - prev) / prev) * 100;
            }

            var result = new DashboardViewModel
            {
                Data = data,
                TotalSales = latest,
                GrowthPercentage = growth,
                GrowthErrorMessage = error,
            };

            return result;
        }

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
