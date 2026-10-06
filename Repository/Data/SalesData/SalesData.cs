using ERP.Repository.Configuration.Enum;
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
        public IQueryable<OrderLine> FilteringQuery(
            IQueryable<OrderLine> query,
            string? name,
            int filter,
            int orderTypeId
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = SearchPattern.TryParseId(name, "SAL", out var id);

                query = query.Where(o =>
                    EF.Functions.ILike(o.Product.Name, pattern)
                    || EF.Functions.ILike(o.Order.CustomerName, pattern)
                    || EF.Functions.ILike(o.Order.OrderType.Type, pattern)
                    || (hasId && o.OrderId == id)
                );
            }

            if (orderTypeId > 0)
            {
                query = query.Where(o => o.Order.OrderTypeId == orderTypeId);
            }

            if (Enum.IsDefined(typeof(OrdersFilter), filter))
            {
                var selectedFilter = (OrdersFilter)filter;

                switch (selectedFilter)
                {
                    case OrdersFilter.PRDATOZ:
                        query = query.OrderBy(o => o.Product.Name);
                        break;
                    case OrdersFilter.PRDZTOA:
                        query = query.OrderByDescending(o => o.Product.Name);
                        break;
                    case OrdersFilter.QHIGH:
                        query = query.OrderByDescending(o => o.Quantity);
                        break;
                    case OrdersFilter.QLOW:
                        query = query.OrderBy(o => o.Quantity);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(o => o.Order.Created_At);
            }

            return query;
        }

        public async Task<IEnumerable<SaleTotalViewModel>> GetOrdersWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            CancellationToken cancellation = default
        )
        {
            var orders = BaseQuery<OrderLine>(false)
                .Where(o => o.Order.OrderStatusId == (int)OrderStatusEnum.Completed);

            orders = FilteringQuery(orders, name, filter, orderTypeId);

            var groupedOrders = orders
                .Include(o => o.Product)
                .Include(o => o.Order.OrderType)
                .Include(o => o.Order.OrderStatus)
                .Include(o => o.Order.DeliveryDriver)
                .GroupBy(o => o.OrderId)
                .Select(o => new SaleTotalViewModel
                {
                    Id = o.Key,
                    OrderType = o.First().Order.OrderType.Type,
                    OrderStatus = o.First().Order.OrderStatus.Status,
                    DriverName =
                        o.First().Order.DeliveryDriver == null
                            ? ""
                            : string.Concat(
                                o.First().Order.DeliveryDriver.FirstName,
                                " ",
                                o.First().Order.DeliveryDriver.LastName
                            ),
                    CustomerName = o.First().Order.CustomerName,
                    PickUpAddress = o.First().Order.PibkupAddress,
                    DeliveryAddress = o.First().Order.DeliveryAddress,
                    Orders = o.GroupBy(g => g.ProductId)
                        .Select(g => new OrderViewModel
                        {
                            ProductName = g.First().Product.Name,
                            Quantity = g.Sum(x => x.Quantity),
                            Price = g.First().Product.Price,
                            TotalAmount = g.Sum(x => x.Amount),
                        })
                        .ToList(),
                    Total = o.Sum(g => g.Amount),
                    Created_At = o.First().Created_At,
                });

            var result = await groupedOrders
                .OrderByDescending(o => o.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            return result;
        }

        public async Task<int> OrdersCount(string? name, int filter, int orderTypeId)
        {
            var orders = BaseQuery<OrderLine>(false)
                .Where(o => o.Order.OrderStatusId == (int)OrderStatusEnum.Completed);

            orders = FilteringQuery(orders, name, filter, orderTypeId);

            var result = await orders.Distinct().GroupBy(s => s.OrderId).CountAsync();

            return result;
        }
    }
}
