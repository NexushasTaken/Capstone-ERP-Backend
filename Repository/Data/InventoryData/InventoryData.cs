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

        public IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string name, int filter, int statusId, int wareHousePresent)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(i => i.Name.Contains(name));
            }

            if(statusId > 0)
            {
                query = query.Where(i => i.StatusId == statusId);
            }

            if(wareHousePresent == 1)
            {
                query = query.Where(i => i.WarehouseId == null);
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
                query = query.OrderByDescending(i => i.Created_At);
            }

            return query;
        }


        public async Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int wareHousePresent, CancellationToken cancellationToken)
        {
            var inventories = BaseQuery<Inventory>(false).Where(i => i.IsActive == true);

            inventories = FilteringQuery(inventories,name,filter,statusId,wareHousePresent);

            var result = await inventories.Include(i => i.Warehouse).Include(i => i.InventoryStatus).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<int> InventoryCount(string name, int filter, int statusId, int wareHousePresent)
        {
            var count = BaseQuery<Inventory>(false).Where(i => i.IsActive == true);

            count = FilteringQuery(count,name,filter, statusId, wareHousePresent);

            var result = await count.CountAsync();

            return result;
        }

        public async Task<Inventory> GetInventoryWithTracking(int id)
        {
            var inventory = await BaseQuery<Inventory>(true).FirstOrDefaultAsync(i => i.Id == id && i.IsActive == true);

            return inventory;
        }

        public async Task<ICollection<Inventory>> GetInventoryWithWareHouseId(int id)
        {
            var inventory = await BaseQuery<Inventory>(true).Where(i => i.IsActive == true && i.WarehouseId == id).ToListAsync();

            return inventory;
        }

        public async Task<int> GetInventoryWithNoWareHouseCountWithoutTracking()
        {
            var count = await BaseQuery<Inventory>(false).Where(i => i.WarehouseId == null && i.IsActive == true).CountAsync();

            return count;
        }

        public async Task<int> GetStatusCountInventory(int statudId)
        {
            var count = await BaseQuery<Inventory>(false).Where(i => i.StatusId == statudId).CountAsync();

            return count;
        }

        #endregion

        #region Damaged Inventory

        #endregion

        #region Warehouse
        public async Task<int> GetIndividualWarehouseCurrentCapacityWithoutTracking(int id)
        {
            var capacity = await BaseQuery<Inventory>(false).Where(w => w.WarehouseId == id && w.IsActive == true).CountAsync();

            return capacity;
        }

        public async Task<List<Warehouse>> GetWarehousesWithoutTracking()
        {
            var wareHouse = await BaseQuery<Warehouse>(false).Where(w => w.IsActive == true).ToListAsync();

            return wareHouse;
        }

        public async Task<int> GetIndividualWarehousesMaxCapacityWithoutTracking(int id)
        {
            var wareHouse = await BaseQuery<Warehouse>(false).FirstOrDefaultAsync(w => w.Id == id && w.IsActive == true);

            return wareHouse.Capacity;
        }

        public async Task<Warehouse> GetIndividualWareHouseWithTracking(int id)
        {
            var wareHouse = await BaseQuery<Warehouse>(true).FirstOrDefaultAsync(w => w.Id == id && w.IsActive == true);

            return wareHouse;
        }
        #endregion

        #region Inventory Label
        public async Task<IEnumerable<InventoryLabel>> GetLabelForInsert()
        {
            var label = await BaseQuery<InventoryLabel>(false).Where(i => i.Type == "restock" || i.Type == "return" && i.IsActive == true).ToListAsync();

            return label;
        }
        #endregion

        #region Damage Inventory
        public async Task<IEnumerable<DamagedInventory>> GetDamageInventoryWithoutTracking(int id)
        {
            var inventory = await BaseQuery<DamagedInventory>(false).Where(d => d.InventoryId == id && d.IsActive == true).OrderByDescending(d => d.Created_At).ToListAsync();

            return inventory;
        }
        #endregion

        #region Transaction
        public async Task<IEnumerable<InventoryTransaction>> GetItemTransactionWithoutTracking(int id)
        {
            var transac = await BaseQuery<InventoryTransaction>(false).Include(t => t.InventoryLabel).Where(t => t.InventoryId == id && t.IsActive == true).ToListAsync();

            return transac;
        }
        #endregion
    }
}
 