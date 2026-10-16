using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.DashboardData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.DashboardData
{
    public class DashBoardData(DatabaseContext _context) : BaseData(_context), IDashboardData
    {
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
