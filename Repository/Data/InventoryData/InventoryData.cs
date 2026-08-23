using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace ERP.Repository.Data.InventoryData
{
    public class InventoryData(DatabaseContext _context) : BaseData(_context), IInventoryData
    {

        #region Inventory

        public IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string searchString, int filter, int statusId)
        {
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(i => i.Name.Contains(searchString));
            }

            if(statusId > 0)
            {
                query = query.Where(i => i.StatusId == statusId);
            }

            if (Enum.IsDefined(typeof(InventoryFilter), filter))
            {
                var selectedFilter = (InventoryFilter)filter;

                switch (selectedFilter)
                {
                    case InventoryFilter.ATOZ:
                        query = query.OrderBy(i => i.Name);
                        break;
                    case InventoryFilter.ZTOA:
                        query = query.OrderByDescending(i => i.Name);
                        break;
                    case InventoryFilter.QHIGH:
                        query = query.OrderByDescending(i => i.Quantity);
                        break;
                    case InventoryFilter.QLOW:
                        query = query.OrderBy(i => i.Quantity);
                        break;
                    case InventoryFilter.RPOINT:
                        query = query.OrderBy(i => i.ReorderPoint);
                        break;
                    case InventoryFilter.WHOUSE:
                        query = query.OrderBy(i => i.WarehouseId);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(i => i.Id);
            }

            return query;
        }


        public async Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(int page, int pageSize, string? searchString, int filter, int statusId, CancellationToken cancellationToken)
        {
            var inventories = BaseQuery<Inventory>(false).Where(i => i.IsActive == true);

            inventories = FilteringQuery(inventories,searchString,filter,statusId);

            var result = await inventories.Include(i => i.Warehouse).Include(i => i.InventoryStatus).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<int> InventoryCount(string searchString, int filter, int statusId)
        {
            var count = BaseQuery<Inventory>(false).Where(i => i.IsActive == true);

            count = FilteringQuery(count,searchString,filter, statusId);

            var result = await count.CountAsync();

            return result;
        }

        public async Task<Inventory> GetInventoryWithTracking(int id)
        {
            var inventory = await BaseQuery<Inventory>(true).FirstOrDefaultAsync(i => i.Id == id && i.IsActive == true);

            return inventory;
        }

        public async Task<int> GetStatusCountInventory(int statudId)
        {
            var count = await BaseQuery<Inventory>(false).Where(i => i.StatusId == statudId).CountAsync();

            return count;
        }

        #endregion

        #region Damaged Inventory

        #endregion
    }
}
 