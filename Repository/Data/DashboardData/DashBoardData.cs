using ERP.Repository.Configuration.Enum;
using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.DashboardData
{
    public class DashBoardData(DatabaseContext _context) : BaseData(_context), IDashboardData
    {



        public async Task<IEnumerable<MonthsDataViewModel>> OverView(DateTime from, DateTime to, CancellationToken cancellation)
        {
            var start = new DateTime(from.Year, from.Month, from.Day, 0,0,0, DateTimeKind.Utc);
            var end = new DateTime(to.Year, to.Month, to.Day,0,0,0,DateTimeKind.Utc);
            var data = await BaseQuery<OrderLine>(false).Include(o => o.Order).Where(s => s.Created_At >= start && s.Created_At < end && s.Order.OrderStatusId == (int)OrderStatusEnum.Completed).ToListAsync(cancellation);

            var result = data
                .GroupBy(s => new { s.Created_At.Value.Year, s.Created_At.Value.Month })
                .OrderBy(g => g.Key.Month)
                .Select(g => new MonthsDataViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    Data = g.Sum(x => x.Amount)
                });

            return result;
        }

        public async Task<IEnumerable<MonthsDataViewModel>> PrevMonths(DateTime from, DateTime to, CancellationToken cancellation)
        {

            TimeSpan span = to - from;

            to = from;

            from = from - span;

            var start = new DateTime(from.Year,from.Month,from.Day,0,0,0,DateTimeKind.Utc);
            var end = new DateTime(to.Year,to.Month,to.Day,0,0,0,DateTimeKind.Utc);

            var data = await BaseQuery<OrderLine>(false).Where(s => s.Created_At >= start && s.Created_At < end).ToListAsync(cancellation);

            var result = data
                .GroupBy(s => new { s.Created_At.Value.Year, s.Created_At.Value.Month })
                .OrderBy(g => g.Key.Month)
                .Select(g => new MonthsDataViewModel
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    Data = g.Sum(x => x.Amount)
                });

            return result;
        }

        public async Task<int> TotalStock()
        {
            var result = await BaseQuery<Inventory>(false).Where(i => i.IsActive == true).SumAsync(i => i.Quantity);

            return result;
        }

        public async Task<IEnumerable<InventoryStatusTotalViewModel>> InventoryStatusOverView()
        {
            var result = await BaseQuery<Inventory>(false).Where(i => i.IsActive == true).GroupBy(i =>  new { i.StatusId, Stat = i.InventoryStatus.Status}).Select(i => new InventoryStatusTotalViewModel
            {
                Status = i.Key.Stat,
                Total = i.Count()
            }).ToListAsync();

            return result;
        }
    }
}
