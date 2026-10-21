using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.PurchaseData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Products;
using ERP.Repository.Model.Purchases;
using ERP.Repository.ViewModel.Purchases;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.PurchaseData
{
    public class PurchaseData(DatabaseContext _context) : BaseData(_context), IPurchaseData
    {
        private IQueryable<Purchase> FilteringQuery(string? name, int statusId)
        {
            var query = BaseQuery<Purchase>(false);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "PO", out var id);

                query = query.Where(p =>
                    EF.Functions.ILike(p.SupplierName, pattern)
                    || p.Lines.Any(l => EF.Functions.ILike(l.Product!.Name, pattern))
                    || (hasId && p.Id == id)
                );
            }

            if (statusId > 0)
            {
                query = query.Where(p => p.PurchaseStatusId == statusId);
            }

            return query;
        }

        // filter: 0 = newest order date first, 1 = oldest first. Ties fall back to Id so paging stays stable.
        private static IQueryable<Purchase> SortingQuery(IQueryable<Purchase> query, int filter) =>
            filter == 1
                ? query.OrderBy(p => p.OrderedOn).ThenBy(p => p.Id)
                : query.OrderByDescending(p => p.OrderedOn).ThenByDescending(p => p.Id);

        public async Task<IEnumerable<PurchaseViewModel>> GetPurchasesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int statusId,
            int filter,
            CancellationToken cancellation = default
        )
        {
            return await SortingQuery(FilteringQuery(name, statusId), filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PurchaseViewModel
                {
                    Id = p.Id,
                    SupplierName = p.SupplierName,
                    OrderedOn = p.OrderedOn,
                    Note = p.Note,
                    StatusId = p.PurchaseStatusId,
                    Status = p.PurchaseStatus!.Status,
                    Lines = p
                        .Lines.OrderBy(l => l.Id)
                        .Select(l => new PurchaseLineViewModel
                        {
                            ProductId = l.ProductId,
                            ProductName = l.Product!.Name,
                            Unit = l.Product.Unit!.Name,
                            Quantity = l.Quantity,
                            ReceivedQuantity = l.ReceivedQuantity,
                            UnitCost = l.UnitCost,
                            Total = l.Quantity * l.UnitCost,
                        })
                        .ToList(),
                    Total = p.Lines.Sum(l => l.Quantity * l.UnitCost),
                    ReceivedTotal =
                        p.ReceivedAt == null ? null : p.Lines.Sum(l => (l.ReceivedQuantity ?? 0) * l.UnitCost),
                    Created_At = p.Created_At,
                    ReceivedAt = p.ReceivedAt,
                })
                .ToListAsync(cancellation);
        }

        public async Task<int> PurchaseCount(string? name, int statusId, CancellationToken cancellation = default)
        {
            return await FilteringQuery(name, statusId).CountAsync(cancellation);
        }

        public async Task<IEnumerable<PurchaseStatusCountViewModel>> StatusCount()
        {
            return await BaseQuery<Purchase>(false)
                .GroupBy(p => new { p.PurchaseStatusId, p.PurchaseStatus!.Status })
                .OrderBy(g => g.Key.PurchaseStatusId)
                .Select(g => new PurchaseStatusCountViewModel { Status = g.Key.Status, Count = g.Count() })
                .ToListAsync();
        }

        public async Task<Purchase?> GetPurchaseWithTracking(int id)
        {
            return await BaseQuery<Purchase>(true)
                .Include(p => p.Lines)
                    .ThenInclude(l => l.Product)
                        .ThenInclude(pr => pr!.Unit)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Dictionary<int, string>> GetActiveProductNames(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();

            return await BaseQuery<Product>(false)
                .Where(p => p.IsActive && ids.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Name ?? "");
        }

        // One stock record per product, so this is keyed by product.
        public async Task<Dictionary<int, Inventory>> GetActiveInventoriesByProductWithTracking(
            IEnumerable<int> productIds
        )
        {
            var ids = productIds.Distinct().ToList();

            return await BaseQuery<Inventory>(true)
                .Where(i => i.IsActive && ids.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId);
        }
    }
}
