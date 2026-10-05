using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Enum;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ERP.Repository.Data.InventoryData
{
    public class InventoryData(DatabaseContext _context) : BaseData(_context), IInventoryData
    {

        #region Inventory

        public IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string? name, int filter, int statusId, int wareHousePresent)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "INV", out var id);

                query = query.Where(i =>
                    EF.Functions.ILike(i.Name, pattern) ||
                    (i.Warehouse != null && EF.Functions.ILike(i.Warehouse.Name, pattern)) ||
                    (i.InventoryStatus != null && EF.Functions.ILike(i.InventoryStatus.Status, pattern)) ||
                    (hasId && i.Id == id));
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

        public async Task<IEnumerable<Inventory>> GetInventoriesWithTracking(List<int> Id)
        {
            var inventory = await BaseQuery<Inventory>(true).Where(i => Id.Contains(i.Id) && i.IsActive == true).ToListAsync();

            return inventory;
        }

        public async Task<ICollection<Inventory>> GetInventoryWithWareHouseId(int id)
        {
            var inventory = await BaseQuery<Inventory>(true).Where(i => i.IsActive == true && i.WarehouseId == id).ToListAsync();

            return inventory;
        }

        public async Task<IEnumerable<Inventory>> GetProductInventoryWithTracking(int productId)
        {
            var inventory = await BaseQuery<Inventory>(true).Include(i => i.Warehouse).Where(i => i.ProductId == productId && i.IsActive == true).ToListAsync();

            return inventory;
        }

        public async Task<bool> CheckExistingInventory(string name, int warehouseId)
        {
            var inventory = await BaseQuery<Inventory>(false).FirstOrDefaultAsync(i => i.Name == name && i.WarehouseId == warehouseId && i.IsActive == true);
            return inventory != null;
        }

        #endregion

        #region Damaged Inventory

        #endregion

        #region Warehouse
        private IQueryable<Warehouse> WarehouseFilteringQuery(string? name)
        {
            var query = BaseQuery<Warehouse>(false).Where(w => w.IsActive == true);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = int.TryParse(name.Trim(), out var id);

                query = query.Where(w => EF.Functions.ILike(w.Name, pattern) || EF.Functions.ILike(w.Address, pattern) || (hasId && w.Id == id));
            }

            return query;
        }

        // filter: 0 = newest first, 1 = Id, 2 = Name A-Z, 3 = Name Z-A. Ties fall back to Id so paging stays stable.
        private static IQueryable<Warehouse> WarehouseSortingQuery(IQueryable<Warehouse> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(w => w.Id),
                2 => query.OrderBy(w => w.Name),
                3 => query.OrderByDescending(w => w.Name),
                _ => query.OrderByDescending(w => w.Created_At),
            };

            return sorted.ThenBy(w => w.Id);
        }

        public async Task<IEnumerable<InventoryWareHouseViewModel>> GetWarehousesWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default)
        {
            var wareHouses = await WarehouseSortingQuery(WarehouseFilteringQuery(name), filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new InventoryWareHouseViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Address = w.Address,
                    Stocks = w.Inventory.Count(i => i.IsActive == true),
                    Created_At = w.Created_At
                })
                .ToListAsync(cancellation);

            return wareHouses;
        }

        public async Task<int> WarehouseTotalCount(string? name, CancellationToken cancellation = default)
        {
            return await WarehouseFilteringQuery(name).CountAsync(cancellation);
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
            var label = await BaseQuery<InventoryLabel>(false).Where(i => (i.Type == "restock" || i.Type == "return") && i.IsActive == true).ToListAsync();

            return label;
        }
        #endregion

        #region Damage Inventory
        public async Task<IEnumerable<DamagedInventory>> GetDamageInventoryWithoutTracking(int id)
        {
            var inventory = await BaseQuery<DamagedInventory>(false).Where(d => d.InventoryId == id && d.IsActive == true).OrderByDescending(d => d.Created_At).ToListAsync();

            return inventory;
        }

        public async Task<IEnumerable<InventoryStatusViewModel>> StatusCount()
        {
            var inventory = await BaseQuery<Inventory>(false).GroupBy(i => new { i.StatusId, i.InventoryStatus.Status }).Select(g => new InventoryStatusViewModel
            {
                Status = g.Key.Status,
                Count = g.Count()

            }).ToListAsync();

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

        #region Movement Velocity
        public async Task<IEnumerable<InventoryMovementVelocityViewModel>> GetMovementVelocityWithoutTracking(int cutOffDate, int page, int pageSize)
        {
            var cutOff = DateTime.UtcNow.AddDays(-cutOffDate);
            cutOff = new DateTime(cutOff.Year, cutOff.Month, cutOff.Day, 23,59,59, DateTimeKind.Utc);

                var inventory = BaseQuery<InventoryTransaction>(false).Include(i => i.Inventory).ThenInclude(i => i.Warehouse)
                .Where(t => t.Inventory.Created_At <= cutOff)
                .GroupBy(t => t.InventoryId)
                .Select(t => new
                {
                    ItemId = t.Key,
                    FirstDate = t.Min(t => t.Created_At),
                    LastDate = t.Max(t => t.Created_At),
                    Name = t.First().Inventory.Name,
                    NetMovement = t.Sum(g =>
                    g.InventoryLabelId == (int)InventoryLabelEnum.Purchase || g.InventoryLabelId == (int)InventoryLabelEnum.Return ? -g.QuantityChanged :
                    g.InventoryLabelId == (int)InventoryLabelEnum.Restock || g.InventoryLabelId == (int)InventoryLabelEnum.Damage ? +g.QuantityChanged : 0),
                    WarehouseName = t.First().Inventory.Warehouse.Name != null ? t.First().Inventory.Warehouse.Name : "No Warehouse"
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsEnumerable()
                .Select(x =>
                {
                    var duration = ((x.LastDate.Value - x.FirstDate.Value).Days) + 1;
                    var velocity = duration > 0 ? (double)x.NetMovement / duration : 0;
                    var classification = "";

                    if (velocity > 10)
                    {
                        classification = "Fast";
                    }
                    else if (velocity >= 3)
                    {
                        classification = "Stable";
                    }
                    else
                    {
                        classification = "Slow";
                    }

                    return new InventoryMovementVelocityViewModel
                    {
                        InventoryId = x.ItemId,
                        Name = x.Name,
                        Classification = classification,
                        VelocityMetric = velocity,
                        Warehouse = x.WarehouseName
                    };
                })
                .ToList();

            return inventory;
        }

        public async Task<int> GetMovementVelocityCount(int cutoffDate)
        {
            var cutoff = DateTime.UtcNow.AddDays(-cutoffDate);

            var inventory = await BaseQuery<InventoryTransaction>(false).Include(i => i.Inventory).ThenInclude(i => i.Warehouse)
               .Where(t => t.Inventory.Created_At <= cutoff).GroupBy(t => t.InventoryId).CountAsync();

            return inventory;
        }
        #endregion
    }
}
 