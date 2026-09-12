using ERP.Repository.Interface.Data.DashboardData;
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

        public async Task<IEnumerable<MonthsDataViewModel>> PrevMonths(DateTime from, CancellationToken cancellation)
        {
            var data = await BaseQuery<OrderLine>(false).Where(s => s.Created_At >= from.AddMonths(-3).ToUniversalTime() && s.Created_At < from.ToUniversalTime()).ToListAsync(cancellation);

            var result = data
                .GroupBy(s => new { s.Created_At.Value.Year, s.Created_At.Value.Month })
                .Select(g => new MonthsDataViewModel
                {
                    Data = g.Sum(x => x.Amount)
                });

            return result;
        }
    }
}
