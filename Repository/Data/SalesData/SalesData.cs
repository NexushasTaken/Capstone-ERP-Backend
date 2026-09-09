using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.Sales
{
    public class SalesData(DatabaseContext _context) : BaseData(_context), ISalesData
    {
        public IQueryable<OrderLine> FilteringQuery(IQueryable<OrderLine> query, string? name, int filter, int orderTypeId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(o => o.Product.Name.Contains(name.ToLower()) || o.Order.CustomerName.Contains(name.ToLower()));
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
        public async Task<IEnumerable<OrderLine>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int orderTypeId, CancellationToken cancellation = default)
        {
            var orders = BaseQuery<OrderLine>(false).Where(o => o.Order.OrderStatusId == 1);

            orders = FilteringQuery(orders, name, filter, orderTypeId);

            var result = await orders.Include(o => o.Product)
                .Include(o => o.Order.OrderType)
                .Include(o => o.Order.OrderStatus)
                .Include(o => o.Order.DeliveryDriver)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            return result;
        }

        public async Task<int> OrdersCount(string? name, int filter, int orderTypeId)
        {
            var orders = BaseQuery<OrderLine>(false).Where(o => o.Order.OrderStatusId == 1);

            orders = FilteringQuery(orders, name, filter, orderTypeId);

            var result = await orders.Distinct().CountAsync();

            return result;
        }
    }
}
