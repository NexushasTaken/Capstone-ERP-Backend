using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.OrderData
{
    public class OrderData(DatabaseContext _context) : BaseData(_context), IOrderData
    {
        public IQueryable<OrderLine> FilteringQuery(IQueryable<OrderLine> query, string? name, int filter, int statusId, int orderTypeId)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(o => o.Product.Name.Contains(name.ToLower()) || o.Order.CustomerName.Contains(name.ToLower()));
            }

            if(statusId > 0)
            {
                query = query.Where(o => o.Order.OrderStatusId == statusId);
            }

            if(orderTypeId > 0)
            {
                query = query.Where(o => o.Order.OrderTypeId == orderTypeId);
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
                query = query.OrderByDescending(o => o.Order.Created_At);
            }

            return query;

        }
        public async Task<IEnumerable<OrderTotalViewModel>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellation = default)
        {
            var orders = BaseQuery<OrderLine>(false).Where(o => o.Order.OrderStatusId != 1);

            orders = FilteringQuery(orders, name, filter, statusId, orderTypeId);

            var groupedOrders = orders
                .Include(o => o.Product)
                .Include(o => o.Order.OrderType)
                .Include(o => o.Order.OrderStatus)
                .Include(o => o.Order.DeliveryDriver)
                .GroupBy(o => o.OrderId)
                .Select(o => new OrderTotalViewModel
                {
                    OrderId = o.Key,
                    OrderType = o.First().Order.OrderType.Type,
                    OrderStatus = o.First().Order.OrderStatus.Status,
                    DriverName = o.First().Order.DeliveryDriver == null ? ""
                        : string.Concat(o.First().Order.DeliveryDriver.FirstName, " ", o.First().Order.DeliveryDriver.LastName),
                    CustomerName = o.First().Order.CustomerName,
                    PickUpAddress = o.First().Order.PibkupAddress,
                    DeliveryAddress = o.First().Order.DeliveryAddress,
                    Orders = o.GroupBy(g => g.ProductId)
                        .Select(g => new OrderViewModel
                        {
                            ProductName = g.First().Product.Name,
                            Quantity = g.Sum(x => x.Quantity),
                            Price = g.First().Product.Price,
                            TotalAmount = g.Sum(x => x.Amount)
                        }).ToList(),
                    Total = o.Sum(g => g.Amount),
                    Created_At = o.First().Created_At
                });

            var result = await groupedOrders
                .OrderByDescending(o => o.Created_At)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            return result;
        }


        public async Task<int> OrdersCount(string? name, int filter, int statusId, int orderTypeId)
        {
            var orders = BaseQuery<OrderLine>(false).Where(o => o.Order.OrderStatusId != 1);

            orders = FilteringQuery(orders,name,filter,statusId,orderTypeId);

            var result = await orders.Distinct().CountAsync();

            return result;
        }

        public async Task<IEnumerable<OrderType>> GetOrderTypesWithoutTracking()
        {
            var type = await BaseQuery<OrderType>(false).Where(t => t.IsActive == true).ToListAsync();
            return type;
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatusesWithoutTracking()
        {
            var status = await BaseQuery<OrderStatus>(false).Where(s => s.IsActive == true).ToListAsync();
            return status;
        }

        public async Task<Order> GetSingleOrderWithTracking(int id)
        {
            var order = await BaseQuery<Order>(true).FirstOrDefaultAsync(o => o.IsActive == true && o.Id == id);
            return order;
        }

        public async Task<IEnumerable<DeliveryDriver>> GetDeliveryRidersWithoutTracking()
        {
            var riders = await BaseQuery<DeliveryDriver>(false).Where(r => r.IsActive == true).ToListAsync();
            return riders;
        }
        public async Task<DeliveryDriver> ValidateDriverWithoutTracking(int? driverId)
        {
            var driver = await BaseQuery<DeliveryDriver>(false).FirstOrDefaultAsync(d => d.IsActive == true && d.Id == driverId);
            return driver;
        }

        public async Task<IEnumerable<OrderLine>> GetOrderLinesWithTracking(int id)
        {
            var order = await BaseQuery<OrderLine>(true).Where(o => o.OrderId == id).Include(o => o.Product).Include(o => o.Inventory).Include(o => o.Order).ToListAsync();

            return order;
        }
    }
}