using System.Security.Cryptography;
using System.Threading.Tasks;
using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.InventoryData
{
    public class InventoryData(DatabaseContext _context) : BaseData(_context), IInventoryData
    {
        #region Inventory

        public IQueryable<Inventory> FilteringQuery(
            IQueryable<Inventory> query,
            string? name,
            int filter,
            int statusId,
            InventoryListFilter listFilter
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "INV", out var id);

                query = query.Where(i =>
                    (i.Product != null && EF.Functions.ILike(i.Product.Name, pattern))
                    || (i.InventoryStatus != null && EF.Functions.ILike(i.InventoryStatus.Status, pattern))
                    || (hasId && i.Id == id)
                );
            }

            if (statusId > 0)
            {
                query = query.Where(i => i.StatusId == statusId);
            }

            if (listFilter.CategoryId > 0)
            {
                query = query.Where(i => i.Product.CategoryId == listFilter.CategoryId);
            }

            if (listFilter.MinQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity >= listFilter.MinQuantity.Value);
            }

            if (listFilter.MaxQuantity.HasValue)
            {
                query = query.Where(i => i.Quantity <= listFilter.MaxQuantity.Value);
            }

            if (Enum.IsDefined(typeof(InventoryFilter), filter))
            {
                var selectedFilter = (InventoryFilter)filter;

                switch (selectedFilter)
                {
                    case InventoryFilter.ATOZ:
                        query = query.OrderBy(i => i.Product.Name);
                        break;
                    case InventoryFilter.ZTOA:
                        query = query.OrderByDescending(i => i.Product.Name);
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
                }
            }
            else
            {
                query = query.OrderByDescending(i => i.Created_At);
            }

            // ties fall back to Id so paging stays stable
            return ((IOrderedQueryable<Inventory>)query).ThenBy(i => i.Id);
        }

        public async Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            int statusId,
            InventoryListFilter listFilter,
            CancellationToken cancellationToken
        )
        {
            var inventories = BaseQuery<Inventory>(false).Where(i => i.IsActive);

            inventories = FilteringQuery(inventories, name, filter, statusId, listFilter);

            var result = await inventories
                .Include(i => i.InventoryStatus)
                .Include(i => i.Product)
                    .ThenInclude(p => p.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return result;
        }

        public async Task<int> InventoryCount(string? name, int filter, int statusId, InventoryListFilter listFilter)
        {
            var count = BaseQuery<Inventory>(false).Where(i => i.IsActive);

            count = FilteringQuery(count, name, filter, statusId, listFilter);

            var result = await count.CountAsync();

            return result;
        }

        public async Task<Inventory> GetInventoryWithTracking(int id)
        {
            var inventory = await BaseQuery<Inventory>(true)
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id && i.IsActive);

            return inventory;
        }

        public async Task<IEnumerable<Inventory>> GetInventoriesWithTracking(List<int> Id)
        {
            var inventory = await BaseQuery<Inventory>(true).Where(i => Id.Contains(i.Id) && i.IsActive).ToListAsync();

            return inventory;
        }

        public async Task<IEnumerable<Inventory>> GetProductInventoryWithTracking(int productId)
        {
            var inventory = await BaseQuery<Inventory>(true)
                .Where(i => i.ProductId == productId && i.IsActive)
                .ToListAsync();

            return inventory;
        }

        public async Task<bool> CheckExistingInventory(int productId)
        {
            return await BaseQuery<Inventory>(false).AnyAsync(i => i.ProductId == productId && i.IsActive);
        }

        #endregion

        #region Damaged Inventory

        #endregion

        #region Inventory Label
        public async Task<IEnumerable<InventoryLabel>> GetLabelForInsert()
        {
            var label = await BaseQuery<InventoryLabel>(false)
                .Where(i => i.Type == "restock" || i.Type == "return")
                .ToListAsync();

            return label;
        }
        #endregion

        #region Damage Inventory
        public async Task<IEnumerable<DamagedInventory>> GetDamageInventoryWithoutTracking(int id)
        {
            var inventory = await BaseQuery<DamagedInventory>(false)
                .Where(d => d.InventoryId == id)
                .OrderByDescending(d => d.Created_At)
                .ToListAsync();

            return inventory;
        }

        public async Task<IEnumerable<InventoryStatusViewModel>> StatusCount()
        {
            var inventory = await BaseQuery<Inventory>(false)
                .Where(i => i.IsActive)
                .GroupBy(i => new { i.StatusId, i.InventoryStatus.Status })
                .Select(g => new InventoryStatusViewModel { Status = g.Key.Status, Count = g.Count() })
                .ToListAsync();

            return inventory;
        }
        #endregion

        #region Transaction
        public async Task<IEnumerable<InventoryTransaction>> GetItemTransactionWithoutTracking(int id)
        {
            var transac = await BaseQuery<InventoryTransaction>(false)
                .Include(t => t.InventoryLabel)
                .Where(t => t.InventoryId == id)
                .ToListAsync();

            return transac;
        }
        #endregion
    }
}
