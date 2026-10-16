using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.UserAccounts;
using ERP.Repository.ViewModel.Orders;
using ERP.Repository.ViewModel.Sales;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.Sales
{
    public class SalesData(DatabaseContext _context) : BaseData(_context), ISalesData
    {
        // from/to are whole Philippine days (to is inclusive), the days the list shows; Created_At is UTC.
        private IQueryable<Sale> FilteringQuery(string? name, int orderTypeId, DateOnly? from, DateOnly? to)
        {
            var query = BaseQuery<Sale>(false);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "SAL", out var id);

                // A product match keeps the whole sale, with all of its lines.
                query = query.Where(s =>
                    EF.Functions.ILike(s.Order.CustomerName, pattern)
                    || EF.Functions.ILike(s.Order.OrderType.Type, pattern)
                    || s.Order.OrderLines.Any(l => EF.Functions.ILike(l.Product.Name, pattern))
                    || (hasId && s.Id == id)
                );
            }

            if (orderTypeId > 0)
            {
                query = query.Where(s => s.Order.OrderTypeId == orderTypeId);
            }

            if (from != null)
            {
                var start = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset;
                query = query.Where(s => s.Created_At >= start);
            }

            if (to != null)
            {
                var end = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset;
                query = query.Where(s => s.Created_At < end);
            }

            return query;
        }

        // filter: 0 = newest first, 1 = oldest first, 2 = total high-low, 3 = total low-high,
        // 4 = customer A-Z, 5 = customer Z-A. Ties fall back to Id so paging stays stable.
        private static IQueryable<Sale> SortingQuery(IQueryable<Sale> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(s => s.Created_At),
                2 => query.OrderByDescending(s => s.Total),
                3 => query.OrderBy(s => s.Total),
                4 => query.OrderBy(s => s.Order.CustomerName),
                5 => query.OrderByDescending(s => s.Order.CustomerName),
                _ => query.OrderByDescending(s => s.Created_At),
            };

            return sorted.ThenBy(s => s.Id);
        }

        public async Task<IEnumerable<SaleTotalViewModel>> GetSalesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellation = default
        )
        {
            var result = await SortingQuery(FilteringQuery(name, orderTypeId, from, to), filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SaleTotalViewModel
                {
                    Id = s.Id,
                    OrderType = s.Order.OrderType.Type,
                    OrderStatus = s.Order.OrderStatus.Status,
                    DriverName =
                        s.Order.DeliveryDriver == null
                            ? ""
                            : string.Concat(s.Order.DeliveryDriver.FirstName, " ", s.Order.DeliveryDriver.LastName),
                    CustomerName = s.Order.CustomerName,
                    DeliveryAddress = s.Order.DeliveryAddress,
                    Orders = s
                        .Order.OrderLines.GroupBy(l => l.ProductId)
                        .Select(g => new SaleLineViewModel
                        {
                            ProductId = g.Key,
                            ProductName = g.First().Product.Name,
                            Quantity = g.Sum(x => x.Quantity),
                            Price = g.First().Product.Price,
                            TotalAmount = g.Sum(x => x.Amount),
                        })
                        .ToList(),
                    Subtotal = s.Order.OrderLines.Sum(l => l.Amount),
                    DiscountPercent = s.Order.DiscountPercent,
                    DiscountType = s.Order.DiscountType,
                    Total = s.Total,
                    Refunded = s.Returns.Sum(r => r.Refund),
                    Created_At = s.Created_At,
                })
                .ToListAsync(cancellation);

            // Units already returned per sale and product, for the page's sales only.
            var saleIds = result.Select(s => s.Id).ToList();
            var returned = await BaseQuery<SaleReturn>(false)
                .Where(r => saleIds.Contains(r.SaleId))
                .GroupBy(r => new { r.SaleId, r.ProductId })
                .Select(g => new
                {
                    g.Key.SaleId,
                    g.Key.ProductId,
                    Quantity = g.Sum(r => r.Quantity),
                })
                .ToListAsync(cancellation);

            foreach (var sale in result)
            {
                // The stored total is what was charged, so the discount shown is whatever makes up the difference.
                sale.DiscountAmount = sale.Subtotal - sale.Total;
                sale.TotalAfterRefunds = sale.Total - sale.Refunded;

                foreach (var line in sale.Orders)
                {
                    line.SoldUnitPrice = line.Quantity == 0 ? 0 : line.TotalAmount / line.Quantity;
                    line.ReturnedQuantity = returned
                        .Where(r => r.SaleId == sale.Id && r.ProductId == line.ProductId)
                        .Sum(r => r.Quantity);
                }
            }

            return result;
        }

        public async Task<int> SalesCount(
            string? name,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellation = default
        )
        {
            return await FilteringQuery(name, orderTypeId, from, to).CountAsync(cancellation);
        }

        public async Task<decimal> SalesTotalAmount(
            string? name,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellation = default
        )
        {
            var sales = FilteringQuery(name, orderTypeId, from, to);
            var total = await sales.SumAsync(s => s.Total, cancellation);
            var refunded = await BaseQuery<SaleReturn>(false)
                .Where(r => sales.Select(s => s.Id).Contains(r.SaleId))
                .SumAsync(r => r.Refund, cancellation);

            return total - refunded;
        }

        // When the oldest and newest sales happened (UTC); both null when there are no sales.
        public async Task<(DateTime? First, DateTime? Last)> SaleDateRange(CancellationToken cancellation = default)
        {
            var first = await BaseQuery<Sale>(false).MinAsync(s => s.Created_At, cancellation);
            var last = await BaseQuery<Sale>(false).MaxAsync(s => s.Created_At, cancellation);

            return (first, last);
        }

        // Every sale in the Philippine days from..to (inclusive), oldest first, with its lines.
        // Uses the list's filter so the report's day boundaries match the Sales list.
        public async Task<List<SaleReportSale>> GetSalesForReport(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellation = default
        )
        {
            return await FilteringQuery(null, 0, from, to)
                .OrderBy(s => s.Created_At)
                .ThenBy(s => s.Id)
                .Select(s => new SaleReportSale
                {
                    Id = s.Id,
                    Created_At = s.Created_At,
                    CustomerName = s.Order.CustomerName,
                    OrderType = s.Order.OrderType.Type,
                    Subtotal = s.Order.OrderLines.Sum(l => l.Amount),
                    Total = s.Total,
                    Refunded = s.Returns.Sum(r => r.Refund),
                    Lines = s
                        .Order.OrderLines.Select(l => new SaleReportLine
                        {
                            ProductId = l.ProductId,
                            ProductName = l.Product.Name,
                            Quantity = l.Quantity,
                            Amount = l.Amount,
                        })
                        .ToList(),
                })
                .ToListAsync(cancellation);
        }

        // The sale with what a return needs: its order lines (product, stock record) and earlier returns.
        public async Task<Sale?> GetSaleForReturnWithTracking(int saleId, CancellationToken cancellation = default)
        {
            return await BaseQuery<Sale>(true)
                .Include(s => s.Order)
                    .ThenInclude(o => o!.OrderLines)
                        .ThenInclude(l => l.Product)
                .Include(s => s.Order)
                    .ThenInclude(o => o!.OrderLines)
                        .ThenInclude(l => l.Inventory)
                .Include(s => s.Returns)
                .FirstOrDefaultAsync(s => s.Id == saleId, cancellation);
        }

        // Search matches the sale id (SAL-12), product or customer; from/to are Philippine days on the return date.
        private IQueryable<SaleReturn> ReturnsFilteringQuery(string? name, DateOnly? from, DateOnly? to)
        {
            var query = BaseQuery<SaleReturn>(false);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "SAL", out var id);

                query = query.Where(r =>
                    EF.Functions.ILike(r.Product.Name, pattern)
                    || EF.Functions.ILike(r.Sale.Order.CustomerName, pattern)
                    || (hasId && r.SaleId == id)
                );
            }

            if (from != null)
            {
                var start = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset;
                query = query.Where(r => r.Created_At >= start);
            }

            if (to != null)
            {
                var end = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset;
                query = query.Where(r => r.Created_At < end);
            }

            return query;
        }

        // Newest first.
        public async Task<IEnumerable<SaleReturnViewModel>> GetReturnsWithoutTracking(
            int page,
            int pageSize,
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellation = default
        )
        {
            var good = (int)ReturnConditionEnum.Good;
            var users = BaseQuery<UserAccount>(false);

            return await ReturnsFilteringQuery(name, from, to)
                .OrderByDescending(r => r.Created_At)
                .ThenBy(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new SaleReturnViewModel
                {
                    Id = r.Id,
                    SaleId = r.SaleId,
                    ProductName = r.Product.Name,
                    CustomerName = r.Sale.Order.CustomerName,
                    Quantity = r.Quantity,
                    Condition = r.ConditionId == good ? "good" : "damaged",
                    Refund = r.Refund,
                    Reason = r.Reason,
                    ProcessedBy = users
                        .Where(u => u.Id == r.Created_By)
                        .Select(u => string.Concat(u.UserInformation.FirstName, " ", u.UserInformation.LastName))
                        .FirstOrDefault(),
                    Created_At = r.Created_At,
                })
                .ToListAsync(cancellation);
        }

        public async Task<int> ReturnsCount(
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellation = default
        )
        {
            return await ReturnsFilteringQuery(name, from, to).CountAsync(cancellation);
        }
    }
}
