using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.DashboardData
{
    public class DashBoardData(DatabaseContext _context) : BaseData(_context), IDashboardData
    {
        public async Task<IEnumerable<MonthsDataViewModel>> OverView(
            DateTime from,
            DateTime to,
            CancellationToken cancellation
        )
        {
            // from and to are whole days, both included. Days and months are Philippine time, as on the
            // Sales list, so a month here matches its total there.
            var start = new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, DateTimeKind.Utc) - DemandWeek.PhOffset;
            var end =
                new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(1) - DemandWeek.PhOffset;
            var data = await BaseQuery<Sale>(false)
                .Where(s => s.Created_At >= start && s.Created_At < end)
                .ToListAsync(cancellation);

            var result = data.Select(s => (Sold: s.Created_At!.Value + DemandWeek.PhOffset, s.Total))
                .GroupBy(s => new { s.Sold.Year, s.Sold.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new MonthsDataViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    Data = g.Sum(x => x.Total),
                });

            return result;
        }

        public async Task<IEnumerable<MonthsDataViewModel>> PrevMonths(
            DateTime from,
            DateTime to,
            CancellationToken cancellation
        )
        {
            // The same number of days just before from (both ends included), counted exactly like OverView.
            var days = (to.Date - from.Date).Days + 1;

            return await OverView(from.Date.AddDays(-days), from.Date.AddDays(-1), cancellation);
        }

        public async Task<int> TotalStock()
        {
            var result = await BaseQuery<Inventory>(false).Where(i => i.IsActive).SumAsync(i => i.Quantity);

            return result;
        }

        public async Task<IEnumerable<InventoryStatusTotalViewModel>> InventoryStatusOverView()
        {
            var result = await BaseQuery<Inventory>(false)
                .Where(i => i.IsActive)
                .GroupBy(i => new { i.StatusId, Stat = i.InventoryStatus.Status })
                .Select(i => new InventoryStatusTotalViewModel { Status = i.Key.Stat, Total = i.Count() })
                .ToListAsync();

            return result;
        }
    }
}
