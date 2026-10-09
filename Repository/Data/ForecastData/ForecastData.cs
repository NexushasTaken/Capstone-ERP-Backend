using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.Forecast;
using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Forecast;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ForecastData
{
    public class ForecastData(DatabaseContext _context) : BaseData(_context), IForecastData
    {
        // Demand = units sold - units returned. Sales are Purchase entries (-quantity, written when an
        // order is completed); every Return entry counts as returned, whatever its sign
        // (Return Stock writes +quantity, Mark as Damaged -> Return Item writes -quantity).
        // Restocks and damage are not demand.
        public async Task<List<DemandEntryViewModel>> DemandEntries(int? productId = null)
        {
            var purchase = (int)InventoryLabelEnum.Purchase;
            var returned = (int)InventoryLabelEnum.Return;

            var query = BaseQuery<InventoryTransaction>(false)
                .Where(t => t.IsActive == true && t.Created_At != null)
                .Where(t => t.InventoryLabelId == purchase || t.InventoryLabelId == returned);

            if (productId > 0)
            {
                query = query.Where(t => t.Inventory!.ProductId == productId);
            }

            return await query
                .Select(t => new DemandEntryViewModel
                {
                    ProductId = t.Inventory!.ProductId,
                    CreatedAt = t.Created_At!.Value,
                    Units = t.InventoryLabelId == purchase ? -t.QuantityChanged : -Math.Abs(t.QuantityChanged),
                })
                .ToListAsync();
        }

        // Active products with their stock in every warehouse added together
        public async Task<List<ForecastProductViewModel>> ForecastProducts()
        {
            return await BaseQuery<Product>(false)
                .Where(p => p.IsActive == true)
                .Select(p => new ForecastProductViewModel
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    FirstStocked = p.Inventory.Min(i => i.Created_At),
                    StockOnHand = p.Inventory.Where(i => i.IsActive == true).Sum(i => i.Quantity),
                })
                .ToListAsync();
        }

        public async Task<DateTime?> LatestForecastTime()
        {
            return await BaseQuery<ForecastResult>(false).Where(f => f.IsActive == true).MaxAsync(f => f.Created_At);
        }

        // Every run replaces the previous results
        public async Task ReplaceForecast(IEnumerable<ForecastResult> results)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            await _context.ForecastWeeks.ExecuteDeleteAsync();
            await _context.ForecastResults.ExecuteDeleteAsync();

            _context.AddRange(results);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        // Search by product name; optionally only the products with a suggested order
        private IQueryable<ForecastResult> FilteringQuery(string? search, bool needOrderOnly)
        {
            var query = BaseQuery<ForecastResult>(false).Where(f => f.IsActive == true);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = SearchPattern.Contains(search);
                query = query.Where(f => f.Product != null && EF.Functions.ILike(f.Product.Name, pattern));
            }

            if (needOrderOnly)
            {
                query = query.Where(f => f.SuggestedOrder > 0);
            }

            return query;
        }

        // Soonest to run out first; products not expected to run out last
        public async Task<List<ForecastResult>> GetForecastPage(
            int page,
            int pageSize,
            string? search,
            bool needOrderOnly
        )
        {
            return await FilteringQuery(search, needOrderOnly)
                .Include(f => f.Product)
                .OrderBy(f => f.RunsOutAround == null)
                .ThenBy(f => f.RunsOutAround)
                .ThenByDescending(f => f.SuggestedOrder)
                .ThenBy(f => f.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> ForecastCount(string? search, bool needOrderOnly)
        {
            return await FilteringQuery(search, needOrderOnly).CountAsync();
        }

        public async Task<int> NeedOrderCount()
        {
            return await BaseQuery<ForecastResult>(false)
                .Where(f => f.IsActive == true && f.SuggestedOrder > 0)
                .CountAsync();
        }

        public async Task<ForecastAccuracyViewModel?> Accuracy()
        {
            var totals = await BaseQuery<ForecastResult>(false)
                .Where(f => f.IsActive == true && f.BacktestSold != null)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Sold = g.Sum(f => f.BacktestSold!.Value),
                    Ai = g.Sum(f => f.AiAbsError!.Value),
                    Baseline = g.Sum(f => f.BaselineAbsError!.Value),
                    Count = g.Count(),
                })
                .FirstOrDefaultAsync();

            if (totals == null || totals.Sold <= 0)
            {
                return null;
            }

            return new ForecastAccuracyViewModel
            {
                AiErrorPercent = totals.Ai / totals.Sold * 100,
                BaselineErrorPercent = totals.Baseline / totals.Sold * 100,
                ProductsTested = totals.Count,
            };
        }

        public async Task<ForecastResult?> GetProductForecast(int productId)
        {
            return await BaseQuery<ForecastResult>(false)
                .Include(f => f.Product)
                .Include(f => f.Weeks.Where(w => w.IsActive == true).OrderBy(w => w.WeekStart))
                .Where(f => f.IsActive == true && f.ProductId == productId)
                .FirstOrDefaultAsync();
        }
    }
}
