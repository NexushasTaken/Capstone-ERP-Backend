using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Orders;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Services.Orders
{
    public class OrderService(IOrderData _orders, IInventoryData _inventory, IProductData _product) : IOrderService
    {
        public async Task<OrderPageViewModel> GetOrders(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default)
        {
            GlobalValidation.PageValidation(page, pageSize);

            var result = await _orders.GetOrdersWithoutTracking(page, pageSize, name, filter, statusId, orderTypeId, cancellationToken);
            var totalCount = await _orders.OrdersCount(name, filter, statusId, orderTypeId);


            var orders = result
                .GroupBy(o => o.OrderId)
                .Select(o => new OrderTotalViewModel
                {
                    OrderId = o.First().OrderId,
                    OrderType = o.First().Order.OrderType.Type,
                    OrderStatus = o.First().Order.OrderStatus.Status,
                    DriverName = o.First().Order.DeliveryDriver == null ? "" : string.Concat(o.First().Order.DeliveryDriver.FirstName, " ", o.First().Order.DeliveryDriver.LastName),
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
                }).ToList();


            return new OrderPageViewModel
            {
                Orders = orders,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount
            };
        }

        public async Task<IEnumerable<OrderTypeViewModel>> GetOrderTypes()
        {
            var result = await _orders.GetOrderTypesWithoutTracking();
            var orderTypes = result.Select(o => new OrderTypeViewModel
            {
                Id = o.Id,
                Type = o.Type
            });

            return orderTypes;
        }

        public async Task<IEnumerable<OrderStatusViewModel>> GetOrderStatuses()
        {
            var result = await _orders.GetOrderStatusesWithoutTracking();
            var orderStatuses = result.Select(o => new OrderStatusViewModel
            {
                Id = o.Id,
                Status = o.Status
            });

            return orderStatuses;
        }

        public async Task<IEnumerable<OrderDeliveryRiderViewModel>> GetDeliveryRiders()
        {
            var result = await _orders.GetDeliveryRidersWithoutTracking();
            var deliveryRiders = result.Select(o => new OrderDeliveryRiderViewModel
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName
            });
            return deliveryRiders;
        }

        public async Task<List<string>?> InsertOrder(OrderPostViewModel order)
        {
            OrderValidation.AddNewOrderValidation(order);

            var message = new List<string> { };

            if (order.DeliveryRiderId > 0)
            {
                var driver = await _orders.ValidateDriverWithoutTracking(order.DeliveryRiderId);

                if (driver == null)
                {
                    throw new BadRequest("Invalid Driver");
                }
            }
          
            var header = new Order
            {
                OrderTypeId = order.OrderTypeId,
                OrderStatusId = 3,
                DeliveryDriverId = order.DeliveryRiderId == 0 ? null : order.DeliveryRiderId,
                CustomerName = order.CustomerName.ToLower(),
                PibkupAddress = order.PickUpAddress.ToLower(),
                DeliveryAddress = order.DeliveryAddress.ToLower(),
            };

            await _orders.Save(header);

            var orders = new List<OrderLine> { };

            foreach (var ord in order.OrderLines)
            {
                var product = await _product.GetProductByIdWithoutTracking(ord.ProductId);

                if (product == null)
                {
                    throw new BadRequest("Product Not Found");
                }

                var inventory = await UpdateInventoryBaseOnOrders(ord.ProductId, ord.Quantity);

                foreach (var inv in inventory)
                {
                    if(inv.OrderQuantity > 0)
                    {
                        message.Add($"Insufficient inventory for product {product.Name} at Warehouse {inv.Warehouse}. Unfulfilled Quantity: {inv.OrderQuantity}");
                    }

                    if (!inv.ToSave)
                    {
                        continue;
                    }

                    orders.Add(new OrderLine
                    {
                        OrderId = header.Id,
                        ProductId = ord.ProductId,
                        InventoryId = inv.InventoryId,
                        Quantity = inv.Quantity,
                        Amount = product.Price * inv.Quantity,
                        IsActive = true,
                        Created_At = DateTime.UtcNow,
                    });
                }
            }

            await _orders.SaveMany(orders);

            return message;
        }

        private async Task<List<(int Quantity, int InventoryId, int OrderQuantity, string Warehouse, bool ToSave)>> UpdateInventoryBaseOnOrders(int productId, int quantity)
        {
            var inventory = await _inventory.GetProductInventoryWithTracking(productId);

            if(inventory == null)
            {
                throw new BadRequest("Product Inventory Not Found");
            }

            List<(int Quantity, int InventoryId, int OrderQuantity, string Warehouse, bool ToSave)> result = new List<(int Quantity, int InventoryId, int OrderQuantity, string Warehouse, bool ToSave)> { };

            foreach (var inv in inventory)
            {
                if (quantity <= 0)
                {
                    break;
                }

                if (inv.Quantity <= 0)
                {
                    result.Add((inv.Quantity, inv.Id, quantity, inv.Warehouse.Name == null ? "No Warehouse" : inv.Warehouse.Name, false));
                    continue;
                }

                if (quantity >= inv.Quantity)
                {
                    quantity -= inv.Quantity;
                    result.Add((inv.Quantity, inv.Id, quantity, inv.Warehouse.Name == null ? "No Warehouse" : inv.Warehouse.Name, true));
                    inv.Quantity = 0;
                    inv.StatusId = ReorderRatio.Ratio(inv.Quantity, inv.ReorderPoint);
                }
                else
                {
                    result.Add((quantity, inv.Id, quantity = 0, inv.Warehouse.Name == null ? "No Warehouse" : inv.Warehouse.Name, true));
                    inv.Quantity -= quantity;
                    quantity = 0;
                    inv.StatusId = ReorderRatio.Ratio(inv.Quantity, inv.ReorderPoint);
                }
            }

            await _inventory.SaveChanges();

            return result;
        }
        
        public async Task UpdateOrderStatus(OrderStatusPostViewModel status)
        {
            OrderValidation.UpdateOrderStatusValidation(status);

            var order = await _orders.GetSingleOrderWithTracking(status.OrderId);

            order.OrderStatusId = status.OrderStatusId;

            await _orders.SaveChanges();
        }
    }
}
