using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.OrderData
{
    public class OrderData(DatabaseContext _context) : BaseData(_context), IOrderData
    {
        public IQueryable<Order> FilteringQuery(IQueryable<Order> query, string name, int filter, int statusId, int orderTypeId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(o => o.Product.Name.Contains(name) || o.CustomerName.Contains(name));
            }

            if(statusId > 0)
            {
                query = query.Where(o => o.OrderStatusId == statusId);
            }

            if(orderTypeId > 0)
            {
                query = query.Where(o => o.OrderTypeId == orderTypeId);
            }

            if(Enum.IsDefined(typeof(OrdersFilter), filter))
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
                query = query.OrderByDescending(o => o.Created_At);
            }

            return query;

        }
        public async Task<IEnumerable<Order>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellation = default)
        {
            var orders = BaseQuery<Order>(false);

            orders = FilteringQuery(orders,name,filter,statusId,orderTypeId);

            var result = await orders.Include(o => o.Product).Include(o => o.OrderType).Include(o => o.OrderStatus).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellation);

            return result;
        }
    }
}
