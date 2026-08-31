using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Interface.Orders;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Services.Orders
{
    public class OrderService(IOrderData _orders) : IOrderService
    {
        public async Task<OrderPageViewModel> GetOrders(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default)
        {
            GlobalValidation.PageValidation(page,pageSize);

            var result = await _orders.GetOrdersWithoutTracking(page,pageSize,name,filter,statusId,orderTypeId,cancellationToken);
            var totalCount = await _orders.OrdersCount(name,filter,statusId,orderTypeId);

            var orders = result.Select(o => new OrderViewModel
            {
                Id = o.Id,
                ProductName = o.Product.Name,
                OrderStatus = o.OrderStatus.Status,
                OrderType = o.OrderType.Type,
                DriverName = o.DeliveryDriver == null ? "" : string.Concat(o.DeliveryDriver.FirstName, " ", o.DeliveryDriver.LastName),
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

        public async Task InsertOrder(List<OrderPostViewModel> order)
        {
            OrderValidation.AddNewOrderValidation(order);

            if(order.Count > 1)
            {
                var code = string.Concat("ordr","-",BundleCodeGenerator.GenerateBundleCode());

                var datas = order.Select(o => new Order
                {
                    ProductId = o.ProductId,
                    OrderTypeId = o.OrderTypeId,
                    OrderStatusId = 3,
                    DeliveryDriverId = o.DeliveryRiderId ?? null,
                    Quantity = o.Quantity,
                    CustomerName = o.CustomerName.ToLower(),
                    PibkupAddress = o.PickUpAddress.ToLower(),
                    DeliveryAddress = o.DeliveryAddress.ToLower(),
                    BundleCode = code,
                    Created_At = DateTime.UtcNow,
                    IsActive = true
                });

                await _orders.SaveMany(datas);

                return;
            }

            var data = new Order
            {
                ProductId = order[0].ProductId,
                OrderTypeId = order[0].OrderTypeId,
                OrderStatusId = 3,
                DeliveryDriverId = order[0].DeliveryRiderId ?? null,
                Quantity = order[0].Quantity,
                CustomerName = order[0].CustomerName.ToLower(),
                PibkupAddress = order[0].PickUpAddress.ToLower(),
                DeliveryAddress = order[0].DeliveryAddress.ToLower(),
                BundleCode = null,
                Created_At = DateTime.UtcNow,
                IsActive = true
            };

            await _orders.Save(data);

            return;
        }
        
        public async Task UpdateOrder()
        {

        }
    }
}
