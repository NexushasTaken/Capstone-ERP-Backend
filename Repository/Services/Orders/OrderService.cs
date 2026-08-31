using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Interface.Orders;
using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Services.Orders
{
    public class OrderService(IOrderData _orders) : IOrderService
    {
        public async Task<OrderPageViewModel> GetOrders(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default)
        {
            var result = await _orders.GetOrdersWithoutTracking(page,pageSize,name,filter,statusId,orderTypeId,cancellationToken);
            var totalCount = await _orders.OrdersCount(name,filter,statusId,orderTypeId);

            var orders = result.Select(o => new OrderViewModel
            {
                Id = o.Id,
                ProductName = o.Product.Name,
                OrderStatus = o.OrderStatus.Status,
                OrderType = o.OrderType.Type,
                DriverName = "",
                Quantity = o.Quantity,
                CustomerName = o.CustomerName,
                PickUpAddress = o.PibkupAddress,
                DeliveryAddress = o.DeliveryAddress,
                Amount = o.Product.Price,
                BundleCode = o.BundleCode,
                Created_At = o.Created_At
            });

            var group = orders
                .GroupBy(o => o.BundleCode ?? o.Id.ToString())
                .Select(o => new OrderTotalViewModel
                {
                    Orders = o.ToList(),
                    Total = o.Count() > 1
                    ? o.Sum(o => o.Amount)
                    : o.First().Amount
                }).ToList();

            var final = new OrderPageViewModel
            {
                Orders = group,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount
            };
            return final;
        }
    }
}
