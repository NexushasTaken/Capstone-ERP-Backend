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
            var data = await BaseQuery<OrderLine>(false).Where(s => s.Created_At >= from.ToUniversalTime() && s.Order.Created_At <= to.ToUniversalTime()).ToListAsync(cancellation);

            var result = data
                .GroupBy(s => new { s.Created_At.Value.Year, s.Created_At.Value.Month })
                .Select(g => new MonthsDataViewModel
                {
                    Data = g.Sum(x => x.Amount)
                });

            return result;
        }

        public async Task<IEnumerable<MonthsDataViewModel>> PrevMonths(DateTime from, DateTime to, CancellationToken cancellation)
        {
            var months = ((to.Year - from.Year) * 12) + (to.Month - from.Month);
            var start = from.AddMonths(-months).ToUniversalTime();
            var end = from.ToUniversalTime();
            var data = await BaseQuery<OrderLine>(false).Where(s => s.Created_At >= start && s.Created_At <= end).ToListAsync(cancellation);

            var result = data
                .GroupBy(s => new { s.Created_At.Value.Year, s.Created_At.Value.Month })
                .Select(g => new MonthsDataViewModel
                {
                    Data = g.Sum(x => x.Amount)
                });

            return result;
        }

       public async Task<int> WarehouseTotal()
        {
            var result = await BaseQuery<Warehouse>(false).Where(w => w.IsActive == true).Select(w => w.Capacity).SumAsync();

            return result;
        }

        public async Task<IEnumerable<InventoryStatusTotalViewModel>> InventoryStatusOverView()
        {
            var result = await BaseQuery<Inventory>(false).GroupBy(i => new { i.StatusId, i.InventoryStatus}).Select(i => new InventoryStatusTotalViewModel
            {
                Status = i.Key.InventoryStatus.Status,
                Total = i.Count()
            }).ToArrayAsync();

            return result;
        }
    }
}
