using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Model.Orders;
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
                    PickUpAddress = s.Order.PickUpAddress,
                    DeliveryAddress = s.Order.DeliveryAddress,
                    Orders = s
                        .Order.OrderLines.GroupBy(l => l.ProductId)
                        .Select(g => new OrderViewModel
                        {
                            ProductName = g.First().Product.Name,
                            Quantity = g.Sum(x => x.Quantity),
                            Price = g.First().Product.Price,
                            TotalAmount = g.Sum(x => x.Amount),
                        })
                        .ToList(),
                    Subtotal = s.Order.OrderLines.Sum(l => l.Amount),
                    DiscountPercent = s.Order.DiscountPercent,
                    Total = s.Total,
                    Created_At = s.Created_At,
                })
                .ToListAsync(cancellation);

            foreach (var sale in result)
            {
                // The stored total is what was charged, so the discount shown is whatever makes up the difference.
                sale.DiscountAmount = sale.Subtotal - sale.Total;
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
            return await FilteringQuery(name, orderTypeId, from, to).SumAsync(s => s.Total, cancellation);
        }
    }
}
