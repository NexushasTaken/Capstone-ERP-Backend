using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.Model.Sales;
using ERP.Repository.ViewModel.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.DashboardData
{
    public class DashBoardData(DatabaseContext _context) : BaseData(_context), IDashboardData
    {



        public async Task<IEnumerable<MonthsDataViewModel>> OverView(DateTime from, DateTime to)
        {
            //var data = await BaseQuery<Sale>(false).Include(s => s.Order).Where(s => s.Order.Created_At >= from && s.Order.Created_At <= to ).ToListAsync();

            //var result = data
            //    .GroupBy(s => new { s.Created_At.Value.Year, s.Order.Created_At.Value.Month })
            //    .Select(g => new MonthsDataViewModel
            //    {
            //        Data = g.Sum(x => x.Order.)
            //    });

            throw new NotFiniteNumberException();
        }
    }
}
