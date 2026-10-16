using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.CurrentUser;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Interface.Data.OrderData;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Orders;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Orders;
using FluentValidation;

namespace ERP.Repository.Services.Orders
{
    public class OrderService(
        IOrderData _orders,
        IInventoryData _inventory,
        IProductData _product,
        IDriverData _driver,
        IAuditLogService _auditLog,
        ICurrentUserService _currentUser,
        IValidator<OrderPostViewModel> _orderPostValidator,
        IValidator<OrderPutViewModel> _orderPutValidator,
        IValidator<OrderStatusPostViewModel> _orderStatusValidator,
        IValidator<DeliveryDriverPostViewModel> _driverPostValidator,
        IValidator<DeliveryDriverPatchViewModel> _driverPatchValidator,
        IValidator<DriverImportViewModel> _driverImportValidator
    ) : IOrderService
    {
        #region Orders

        // Secretaries may only pick the legal presets; a custom rate is the owner's call.
        // When editing, a custom discount the owner already set may stay as it is.
        private void EnsureCanSetDiscount(IOrderDiscount discount, Order? current)
        {
            if (discount.DiscountType != DiscountTypes.Custom || _currentUser.Role == "owner")
            {
                return;
            }

            var keepsOwnersDiscount =
                current?.DiscountType == DiscountTypes.Custom && current.DiscountPercent == discount.DiscountPercent;

            if (!keepsOwnersDiscount)
            {
                throw new BadRequest("Only the owner can set a custom discount");
            }
        }

        private static string DiscountSummary(string type, decimal percent) =>
            type == DiscountTypes.None ? "none" : $"{DiscountTypes.Label(type)} {percent}%";

        public async Task<OrderPageViewModel> GetOrders(
            int page,
            int pageSize,
            string? name,
            int filter,
            int statusId,
            int orderTypeId,
            CancellationToken cancellationToken = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var result = await _orders.GetOrdersWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                statusId,
                orderTypeId,
                cancellationToken
            );
            var totalCount = await _orders.OrdersCount(name, filter, statusId, orderTypeId);

            return new OrderPageViewModel
            {
                Orders = result,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount,
            };
        }

        public async Task<IEnumerable<OrderTypeViewModel>> GetOrderTypes()
        {
            var result = await _orders.GetOrderTypesWithoutTracking();
            var orderTypes = result.Select(o => new OrderTypeViewModel { Id = o.Id, Type = o.Type });

            return orderTypes;
        }

        public async Task<IEnumerable<OrderStatusViewModel>> GetOrderStatuses()
        {
            var result = await _orders.GetOrderStatusesWithoutTracking();
            var orderStatuses = result.Select(o => new OrderStatusViewModel { Id = o.Id, Status = o.Status });

            return orderStatuses;
        }

        public async Task<IEnumerable<OrderDeliveryDriverViewModel>> GetDeliveryDrivers()
        {
            var result = await _orders.GetDeliveryDriversWithoutTracking();
            var deliveryDrivers = result.Select(o => new OrderDeliveryDriverViewModel
            {
                Id = o.Id,
                FirstName = o.FirstName,
                LastName = o.LastName,
            });
            return deliveryDrivers;
        }

        public async Task<IEnumerable<OrderStatusCount>> OrdersStatusCount()
        {
            var data = await _orders.StatusCount();

            return data;
        }

        public async Task<List<string>?> InsertOrder(OrderPostViewModel order)
        {
            await _orderPostValidator.EnsureValidAsync(order);
            EnsureCanSetDiscount(order, null);

            var message = new List<string> { };

            if (order.DeliveryDriverId > 0)
            {
                var driver = await _orders.ValidateDriverWithoutTracking(order.DeliveryDriverId);

                if (driver == null)
                {
                    throw new BadRequest("Invalid Driver");
                }
            }

            // Check stock for every product before saving anything, so a shortage creates no order.
            var products = new Dictionary<int, Product>();
            var shortages = new List<string>();

            foreach (
                var (productId, quantity) in order
                    .OrderLines.GroupBy(l => l.ProductId)
                    .Select(g => (g.Key, g.Sum(l => l.Quantity)))
            )
            {
                var product = await _product.GetProductByIdWithoutTracking(productId);

                if (product == null)
                {
                    throw new BadRequest("Product Not Found");
                }

                products[productId] = product;

                var available = (await _inventory.GetProductInventoryWithTracking(productId))
                    .Where(i => i.Quantity > 0)
                    .Sum(i => i.Quantity);

                if (available < quantity)
                {
                    shortages.Add($"Not enough stock for {product.Name}: {available} available, {quantity} needed");
                }
            }

            if (shortages.Count > 0)
            {
                throw new BadRequest(string.Join(". ", shortages));
            }

            var now = DateTime.UtcNow;

            var header = new Order
            {
                OrderTypeId = order.OrderTypeId,
                OrderStatusId = (int)OrderStatusEnum.Processing,
                DeliveryDriverId = order.DeliveryDriverId == 0 ? null : order.DeliveryDriverId,
                CustomerName = order.CustomerName.ToLower(),
                DeliveryAddress = order.DeliveryAddress.ToLower(),
                DiscountPercent = order.DiscountPercent,
                DiscountType = order.DiscountType,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
            };

            await _orders.Save(header);

            var orders = new List<OrderLine> { };

            foreach (var ord in order.OrderLines)
            {
                var product = products[ord.ProductId];

                var inventory = await UpdateInventoryBaseOnOrders(ord.ProductId, ord.Quantity);

                foreach (var inv in inventory)
                {
                    if (inv.OrderQuantity > 0)
                    {
                        message.Add(
                            $"Insufficient inventory for product {product.Name}. Unfulfilled Quantity: {inv.OrderQuantity}"
                        );
                    }

                    if (!inv.ToSave)
                    {
                        continue;
                    }

                    orders.Add(
                        new OrderLine
                        {
                            OrderId = header.Id,
                            ProductId = ord.ProductId,
                            InventoryId = inv.InventoryId,
                            Quantity = inv.Quantity,
                            Amount = product.Price * inv.Quantity,
                            Created_By = _auditLog.CurrentUserId,
                            Created_At = now,
                        }
                    );
                }
            }

            var orderType = (await _orders.GetOrderTypesWithoutTracking())
                .FirstOrDefault(t => t.Id == header.OrderTypeId)
                ?.Type;

            _auditLog.Log(
                AuditModuleEnum.Order,
                AuditActionEnum.Create,
                $"Created {orderType ?? "new"} order #{header.Id} for {header.CustomerName} ({order.OrderLines.Count()} item(s))",
                header.Id,
                now
            );

            await _orders.SaveMany(orders);

            return message;
        }

        public async Task UpdateOrder(OrderPutViewModel order)
        {
            await _orderPutValidator.EnsureValidAsync(order);

            var lines = (await _orders.GetOrderLinesWithTracking(order.OrderId)).ToList();

            if (lines.Count == 0)
            {
                throw new NotFound("Order not found");
            }

            var header = lines.First().Order;

            if (header.OrderStatusId != (int)OrderStatusEnum.Processing)
            {
                throw new BadRequest("Only processing orders can be edited");
            }

            EnsureCanSetDiscount(order, header);

            var isWalkin = header.OrderTypeId == (int)OrderTypeEnum.Walkin;

            if (!isWalkin && order.DeliveryDriverId > 0)
            {
                var driver = await _orders.ValidateDriverWithoutTracking(order.DeliveryDriverId);

                if (driver == null)
                {
                    throw new BadRequest("Invalid Driver");
                }
            }

            var desired = order
                .OrderLines.GroupBy(l => l.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
            var current = lines.GroupBy(l => l.ProductId).ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));
            var deltas = desired
                .Keys.Union(current.Keys)
                .Select(id => (ProductId: id, Delta: desired.GetValueOrDefault(id) - current.GetValueOrDefault(id)))
                .Where(d => d.Delta != 0)
                .ToList();

            // Check every added quantity before touching stock, so a shortage leaves the order unchanged.
            var products = new Dictionary<int, Product>();
            var shortages = new List<string>();

            foreach (var (productId, delta) in deltas.Where(d => d.Delta > 0))
            {
                var product = await _product.GetProductByIdWithoutTracking(productId);

                if (product == null)
                {
                    throw new BadRequest("Product Not Found");
                }

                products[productId] = product;

                var available = (await _inventory.GetProductInventoryWithTracking(productId))
                    .Where(i => i.Quantity > 0)
                    .Sum(i => i.Quantity);

                if (available < delta)
                {
                    shortages.Add($"Not enough stock for {product.Name}: {available} available, {delta} needed");
                }
            }

            if (shortages.Count > 0)
            {
                throw new BadRequest(string.Join(". ", shortages));
            }

            var now = DateTime.UtcNow;
            var newLines = new List<OrderLine>();

            foreach (var (productId, delta) in deltas)
            {
                if (delta < 0)
                {
                    ReturnOrderLineStock(lines.Where(l => l.ProductId == productId), -delta, now);
                    continue;
                }

                var product = products[productId];
                var inventory = await UpdateInventoryBaseOnOrders(productId, delta);

                newLines.AddRange(
                    inventory
                        .Where(inv => inv.ToSave)
                        .Select(inv => new OrderLine
                        {
                            OrderId = header.Id,
                            ProductId = productId,
                            InventoryId = inv.InventoryId,
                            Quantity = inv.Quantity,
                            Amount = product.Price * inv.Quantity,
                            Created_By = _auditLog.CurrentUserId,
                            Created_At = now,
                        })
                );
            }

            var oldDiscount = DiscountSummary(header.DiscountType, header.DiscountPercent);
            var newDiscount = DiscountSummary(order.DiscountType, order.DiscountPercent);

            header.DeliveryDriverId = isWalkin || order.DeliveryDriverId == 0 ? null : order.DeliveryDriverId;
            header.DeliveryAddress = isWalkin ? "" : (order.DeliveryAddress ?? "").ToLower();
            header.DiscountPercent = order.DiscountPercent;
            header.DiscountType = order.DiscountType;
            header.Updated_By = _auditLog.CurrentUserId;
            header.Updated_At = now;

            // Removed lines are hard-deleted, so the audit message is the only record of what changed.
            var productNames = lines
                .GroupBy(l => l.ProductId)
                .ToDictionary(g => g.Key, g => g.First().Product?.Name ?? $"product #{g.Key}");

            foreach (var (productId, product) in products)
            {
                productNames[productId] = product.Name ?? $"product #{productId}";
            }

            var changes = deltas
                .Select(d =>
                {
                    var name = productNames[d.ProductId];
                    var from = current.GetValueOrDefault(d.ProductId);
                    var to = desired.GetValueOrDefault(d.ProductId);

                    return from == 0 ? $"added {name} ×{to}"
                        : to == 0 ? $"removed {name} ×{from}"
                        : $"{name} {from} → {to}";
                })
                .ToList();

            if (oldDiscount != newDiscount)
            {
                changes.Add($"discount {oldDiscount} → {newDiscount}");
            }

            var changeSummary = changes.Count == 0 ? "" : $": {string.Join(", ", changes)}";

            _auditLog.Log(
                AuditModuleEnum.Order,
                AuditActionEnum.Update,
                $"Edited order #{header.Id} for {header.CustomerName}{changeSummary}",
                header.Id,
                now
            );

            await _orders.SaveChanges();
            await _orders.SaveMany(newLines);
        }

        // Gives `quantity` back to stock from the newest lines first. A line that reaches 0 is deleted.
        private void ReturnOrderLineStock(IEnumerable<OrderLine> lines, int quantity, DateTime now)
        {
            foreach (var line in lines.OrderByDescending(l => l.Id))
            {
                if (quantity <= 0)
                {
                    break;
                }

                var taken = Math.Min(quantity, line.Quantity);
                var unitPrice = line.Amount / line.Quantity;

                line.Inventory.Quantity += taken;
                line.Inventory.StatusId = ReorderRatio.Ratio(line.Inventory.Quantity, line.Inventory.ReorderPoint);
                line.Inventory.Updated_By = _auditLog.CurrentUserId;
                line.Inventory.Updated_At = now;

                line.Quantity -= taken;
                line.Amount = unitPrice * line.Quantity;
                line.Updated_By = _auditLog.CurrentUserId;
                line.Updated_At = now;

                if (line.Quantity == 0)
                {
                    _orders.Remove(line);
                }

                quantity -= taken;
            }
        }

        private async Task<
            List<(int Quantity, int InventoryId, int OrderQuantity, bool ToSave)>
        > UpdateInventoryBaseOnOrders(int productId, int quantity)
        {
            var inventory = await _inventory.GetProductInventoryWithTracking(productId);

            if (inventory == null)
            {
                throw new BadRequest("Product Inventory Not Found");
            }

            List<(int Quantity, int InventoryId, int OrderQuantity, bool ToSave)> result = new List<(
                int Quantity,
                int InventoryId,
                int OrderQuantity,
                bool ToSave
            )>
            { };

            foreach (var inv in inventory)
            {
                if (quantity <= 0)
                {
                    break;
                }

                if (inv.Quantity <= 0)
                {
                    result.Add((inv.Quantity, inv.Id, quantity, false));
                    continue;
                }

                if (quantity >= inv.Quantity)
                {
                    quantity -= inv.Quantity;
                    result.Add((inv.Quantity, inv.Id, quantity, true));
                    inv.Quantity = 0;
                    inv.StatusId = ReorderRatio.Ratio(inv.Quantity, inv.ReorderPoint);
                    inv.Updated_By = _auditLog.CurrentUserId;
                    inv.Updated_At = DateTime.UtcNow;
                }
                else
                {
                    result.Add((quantity, inv.Id, 0, true));
                    inv.Quantity -= quantity;
                    quantity = 0;
                    inv.StatusId = ReorderRatio.Ratio(inv.Quantity, inv.ReorderPoint);
                    inv.Updated_By = _auditLog.CurrentUserId;
                    inv.Updated_At = DateTime.UtcNow;
                }
            }

            await _inventory.SaveChanges();

            return result;
        }

        public async Task UpdateOrderStatus(OrderStatusPostViewModel status)
        {
            await _orderStatusValidator.EnsureValidAsync(status);

            var order = await _orders.GetOrderLinesWithTracking(status.OrderId);

            if (order == null || !order.Any())
            {
                throw new BadRequest("Order Not Found");
            }

            var now = DateTime.UtcNow;
            var header = order.First().Order;
            var oldStatusId = header.OrderStatusId;

            // Validate the status is defined
            if (!Enum.IsDefined(typeof(OrderStatusEnum), status.OrderStatusId))
            {
                throw new BadRequest("Invalid Order Status");
            }

            // Validate the transition is allowed
            ValidateStatusTransition(oldStatusId, status.OrderStatusId, header.OrderTypeId);

            header.OrderStatusId = status.OrderStatusId;
            header.Updated_By = _auditLog.CurrentUserId;
            header.Updated_At = now;

            var statuses = await _orders.GetOrderStatusesWithoutTracking();
            var oldStatus = statuses.FirstOrDefault(s => s.Id == oldStatusId)?.Status ?? oldStatusId.ToString();
            var newStatus =
                statuses.FirstOrDefault(s => s.Id == status.OrderStatusId)?.Status ?? status.OrderStatusId.ToString();

            _auditLog.Log(
                AuditModuleEnum.Order,
                AuditActionEnum.StatusChange,
                $"Changed order #{header.Id} status {oldStatus} → {newStatus}",
                header.Id,
                now
            );

            var stat = (OrderStatusEnum)status.OrderStatusId;

            var inventory = order.Select(o => (o.InventoryId, o.Quantity)).ToList();

            switch (stat)
            {
                case OrderStatusEnum.Completed:
                    header.Sale = NewSale(order, header.DiscountPercent, now);
                    await CommitInventoryTransaction(inventory);
                    break;
                case OrderStatusEnum.Cancelled:
                    await RevertInventoryTransaction(inventory);
                    break;
            }

            await _orders.SaveChanges();

            return;
        }

        private static void ValidateStatusTransition(int currentStatusId, int targetStatusId, int orderTypeId)
        {
            var currentStatus = (OrderStatusEnum)currentStatusId;
            var targetStatus = (OrderStatusEnum)targetStatusId;
            var orderType = (OrderTypeEnum)orderTypeId;

            // Same status is not allowed
            if (currentStatus == targetStatus)
            {
                throw new BadRequest($"Order is already in {targetStatus} status");
            }

            // Completed and Cancelled are terminal states
            if (currentStatus == OrderStatusEnum.Completed)
            {
                throw new BadRequest("A completed order can no longer be changed");
            }

            if (currentStatus == OrderStatusEnum.Cancelled)
            {
                throw new BadRequest("A cancelled order can no longer be changed");
            }

            // Shipped is only for Delivery orders
            if (targetStatus == OrderStatusEnum.Shipped && orderType != OrderTypeEnum.Delivery)
            {
                throw new BadRequest("Only delivery orders can be shipped");
            }

            // Shipped cannot go back to Processing
            if (currentStatus == OrderStatusEnum.Shipped && targetStatus == OrderStatusEnum.Processing)
            {
                throw new BadRequest("A shipped order cannot go back to processing");
            }
        }

        // The sale record of a completed order: when it sold, who completed it, and the amount charged.
        private Sale NewSale(IEnumerable<OrderLine> lines, decimal discountPercent, DateTime now)
        {
            var subtotal = lines.Sum(l => l.Amount);

            return new Sale
            {
                Total = subtotal - OrderMath.DiscountAmount(subtotal, discountPercent),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
            };
        }

        private async Task CommitInventoryTransaction(List<(int inventoryId, int quantity)> inventory)
        {
            // stock was already deducted when the order was created, so the current quantity is the level after this sale
            var stock = (
                await _inventory.GetInventoriesWithTracking(inventory.Select(i => i.inventoryId).ToList())
            ).ToDictionary(i => i.Id, i => i.Quantity);

            var transaction = inventory.Select(i => new InventoryTransaction
            {
                InventoryId = i.inventoryId,
                QuantityChanged = -i.quantity,
                StockLevel = stock.GetValueOrDefault(i.inventoryId),
                InventoryLabelId = (int)InventoryLabelEnum.Purchase,
                Created_By = _auditLog.CurrentUserId,
                Created_At = DateTime.UtcNow,
            });

            await _inventory.SaveMany(transaction);

            return;
        }

        private async Task RevertInventoryTransaction(List<(int inventoryId, int quantity)> inventory)
        {
            var inv = await _inventory.GetInventoriesWithTracking(inventory.Select(i => i.inventoryId).ToList());

            foreach (var i in inv)
            {
                var qty = inventory.FirstOrDefault(x => x.inventoryId == i.Id).quantity;
                i.Quantity += qty;
                i.StatusId = ReorderRatio.Ratio(i.Quantity, i.ReorderPoint);
                i.Updated_By = _auditLog.CurrentUserId;
                i.Updated_At = DateTime.UtcNow;
            }

            await _inventory.SaveChanges();

            return;
        }

        #endregion

        #region Drivers

        public async Task<DeliveryDriverPageViewModel> GetDrivers(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        )
        {
            var driver = await _driver.GetDriversWithoutTracking(page, pageSize, name, filter, cancellation);
            var count = await _driver.DriverCount(name, filter);

            var result = driver.Select(d => new DeliveryDriverViewModel
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                Created_At = d.Created_At,
            });

            return new DeliveryDriverPageViewModel
            {
                DeliveryDrivers = result,
                PageCount = (int)Math.Ceiling((double)count / pageSize),
                Rows = count,
            };
        }

        public async Task AddDriver(DeliveryDriverPostViewModel driver)
        {
            await _driverPostValidator.EnsureValidAsync(driver);

            var now = DateTime.UtcNow;

            var data = new DeliveryDriver
            {
                FirstName = driver.FirstName.ToLower(),
                LastName = driver.LastName.ToLower().Trim(),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
            };

            await _driver.Save(data);

            _auditLog.Log(
                AuditModuleEnum.Driver,
                AuditActionEnum.Create,
                $"Added driver {data.FirstName} {data.LastName}".Trim(),
                data.Id,
                now
            );
            await _driver.SaveChanges();

            return;
        }

        // All-or-nothing: every row is validated first, then all drivers are saved in one transaction.
        public async Task<int> ImportDrivers(DriverImportViewModel import)
        {
            await _driverImportValidator.EnsureValidAsync(import);

            var now = DateTime.UtcNow;

            var drivers = import
                .Rows.Select(d => new DeliveryDriver
                {
                    FirstName = d.FirstName!.Trim().ToLower(),
                    LastName = (d.LastName ?? "").Trim().ToLower(),
                    Created_By = _auditLog.CurrentUserId,
                    Created_At = now,
                    IsActive = true,
                })
                .ToList();

            await using var transaction = await _driver.BeginTransaction();

            await _driver.SaveMany(drivers);

            foreach (var data in drivers)
            {
                _auditLog.Log(
                    AuditModuleEnum.Driver,
                    AuditActionEnum.Create,
                    $"Imported driver {data.FirstName} {data.LastName}".Trim(),
                    data.Id,
                    now
                );
            }
            await _driver.SaveChanges();

            await transaction.CommitAsync();

            return drivers.Count;
        }

        public async Task UpdateDriver(DeliveryDriverPatchViewModel driver)
        {
            await _driverPatchValidator.EnsureValidAsync(driver);

            var data = await _driver.GetSingleDriverWithTracking(driver.Id);

            if (data == null)
            {
                throw new NotFound("Driver not found");
            }

            var now = DateTime.UtcNow;
            var oldName = $"{data.FirstName} {data.LastName}".Trim();

            data.FirstName = driver.FirstName.ToLower();
            data.LastName = driver.LastName.ToLower().Trim();
            data.Updated_By = _auditLog.CurrentUserId;
            data.Updated_At = now;

            _auditLog.Log(
                AuditModuleEnum.Driver,
                AuditActionEnum.Update,
                $"Updated driver {oldName} → {data.FirstName} {data.LastName}".Trim(),
                data.Id,
                now
            );

            await _driver.SaveChanges();
        }

        public async Task DeleteDriver(int id)
        {
            var data = await _driver.GetSingleDriverWithTracking(id);

            if (data == null)
            {
                throw new NotFound("Driver not found");
            }
            var now = DateTime.UtcNow;

            data.IsActive = false;
            data.Deleted_By = _auditLog.CurrentUserId;
            data.Deleted_At = now;

            _auditLog.Log(
                AuditModuleEnum.Driver,
                AuditActionEnum.Delete,
                $"Deleted driver {data.FirstName} {data.LastName}".Trim(),
                data.Id,
                now
            );

            await _driver.SaveChanges();
        }

        #endregion
    }
}
