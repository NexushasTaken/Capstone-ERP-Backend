using System.Runtime.ConstrainedExecution;
using ERP.Controllers.InventoryController;
using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using ERP.Repository.ViewModel.Products;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Repository.Services.Inventories
{
    public class InventoryService(
        IInventoryData _inventory,
        IProductData _product,
        IAuditLogService _auditLog,
        IValidator<InventoryPostViewModel> _inventoryPostValidator,
        IValidator<InventoryUpdateViewModel> _inventoryUpdateValidator,
        IValidator<InventoryDamagePostViewModel> _damageValidator,
        IValidator<InventoryTransactionPostViewModel> _transactionValidator,
        IValidator<InventoryWareHousePostViewModel> _warehousePostValidator,
        IValidator<InventoryWareHouseUpdateViewModel> _warehouseUpdateValidator
    ) : IInventoryService
    {
        #region Inventory
        public async Task<InventoryPageViewModel> GetInventories(
            int page,
            int pageSize,
            string name,
            int filter,
            int statusId,
            int wareHousePresent,
            InventoryListFilter listFilter,
            CancellationToken cancellationToken
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var inv = await _inventory.GetInventoriesWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                statusId,
                wareHousePresent,
                listFilter,
                cancellationToken
            );

            var totalCount = await _inventory.InventoryCount(name, filter, statusId, wareHousePresent, listFilter);

            var inventory = inv.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = i.Product?.Name,
                Quantity = i.Quantity,
                ReorderPoint = i.ReorderPoint,
                WarehouseId = i.WarehouseId ?? 0,
                WarehouseName = i.Warehouse?.Name ?? "No Warehouse",
                Status = i.InventoryStatus.Status,
                CategoryName = i.Product?.Category?.Type ?? "No Category",
                Created_At = i.Created_At,
            });

            var result = new InventoryPageViewModel
            {
                inventories = inventory,
                PageCount = (int)Math.Ceiling(totalCount / (double)pageSize),
                Rows = totalCount,
            };

            return result;
        }

        public async Task<IEnumerable<ProductViewModel>> GetAllProductForInsert()
        {
            var product = await _product.GetAllProductForInventoryInsert();

            return product
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category == null ? "No Category" : p.Category.Type,
                    Created_At = p.Created_At,
                })
                .ToList();
        }

        public async Task InsertItem(InventoryPostViewModel inventory)
        {
            await _inventoryPostValidator.EnsureValidAsync(inventory);

            var duplicate = await _inventory.CheckExistingInventory(inventory.ProductId, inventory.WarehouseId);

            if (duplicate)
            {
                throw new BadRequest("This product is already stocked in this warehouse.");
            }

            var product = await _product.GetProductByIdWithoutTracking(inventory.ProductId);

            if (product == null)
            {
                throw new NotFound("Product not found");
            }

            var now = DateTime.UtcNow;

            var inv = new Inventory
            {
                Quantity = inventory.Quantity,
                WarehouseId = inventory.WarehouseId,
                ProductId = inventory.ProductId,
                ReorderPoint = inventory.ReorderPoint,
                StatusId = ReorderRatio.Ratio(inventory.Quantity, inventory.ReorderPoint),
                IsActive = true,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
            };

            await _inventory.Save(inv);

            var warehouse = await _inventory.GetIndividualWareHouseWithTracking(inventory.WarehouseId);

            _auditLog.Log(
                AuditModuleEnum.Inventory,
                AuditActionEnum.Create,
                $"Added {inv.Quantity} '{product.Name}' to {warehouse?.Name ?? "no warehouse"}",
                inv.Id,
                now
            );
            await _inventory.SaveChanges();

            // log the opening stock so the movement history starts from the real quantity
            await InventoryTransaction(
                new InventoryTransactionPostViewModel
                {
                    Id = inv.Id,
                    Quantity = inv.Quantity,
                    Label = (int)InventoryLabelEnum.Restock,
                    StockLevel = inv.Quantity,
                }
            );

            return;
        }

        public async Task Restock(InventoryRestockViewModel inventory)
        {
            var existing = await _inventory.GetInventoryWithTracking(inventory.Id);

            if (existing == null)
            {
                throw new NotFound("Inventory item not found");
            }

            var now = DateTime.UtcNow;

            existing.Quantity += inventory.Quantity;
            existing.Updated_By = _auditLog.CurrentUserId;
            existing.Updated_At = now;

            existing.StatusId = ReorderRatio.Ratio(existing.Quantity, existing.ReorderPoint);

            if (inventory.RestockType == 1)
            {
                _auditLog.Log(
                    AuditModuleEnum.Inventory,
                    AuditActionEnum.IncreaseStock,
                    $"Received {inventory.Quantity} '{ItemLabel(existing)}'",
                    existing.Id,
                    now
                );
            }
            else
            {
                _auditLog.Log(
                    AuditModuleEnum.Inventory,
                    AuditActionEnum.ReturnStock,
                    $"Returned {inventory.Quantity} '{ItemLabel(existing)}' to stock",
                    existing.Id,
                    now
                );
            }

            await _inventory.SaveChanges();

            var transaction = new InventoryTransactionPostViewModel { };

            if (inventory.RestockType == 1)
            {
                transaction = new InventoryTransactionPostViewModel
                {
                    Id = existing.Id,
                    Quantity = +inventory.Quantity,
                    Label = (int)InventoryLabelEnum.Restock,
                    StockLevel = existing.Quantity,
                };
            }
            else
            {
                transaction = new InventoryTransactionPostViewModel
                {
                    Id = existing.Id,
                    Quantity = +inventory.Quantity,
                    Label = (int)InventoryLabelEnum.Return,
                    StockLevel = existing.Quantity,
                };
            }

            await InventoryTransaction(transaction);

            return;
        }

        public async Task DeleteItem(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Id is required");
            }

            var inventory = await _inventory.GetInventoryWithTracking(id);

            if (inventory == null)
            {
                throw new NotFound("Inventory item not found");
            }

            var now = DateTime.UtcNow;

            inventory.IsActive = false;
            inventory.Deleted_By = _auditLog.CurrentUserId;
            inventory.Deleted_At = now;

            _auditLog.Log(
                AuditModuleEnum.Inventory,
                AuditActionEnum.Delete,
                $"Deleted inventory '{ItemLabel(inventory)}'",
                inventory.Id,
                now
            );

            await _inventory.SaveChanges();
        }

        public async Task UpdateInventory(InventoryUpdateViewModel inventory)
        {
            await _inventoryUpdateValidator.EnsureValidAsync(inventory);

            var existing = await _inventory.GetInventoryWithTracking(inventory.Id);

            if (existing == null)
            {
                throw new NotFound("Inventory Item Not Found");
            }

            var now = DateTime.UtcNow;

            // product and warehouse are fixed; stock only moves between warehouses through a transfer
            var message =
                existing.ReorderPoint != inventory.ReorderPoint
                    ? $"Updated inventory '{ItemLabel(existing)}': reorder point {existing.ReorderPoint} → {inventory.ReorderPoint}"
                    : $"Updated inventory '{ItemLabel(existing)}' (no changes)";

            existing.ReorderPoint = inventory.ReorderPoint;
            existing.Updated_By = _auditLog.CurrentUserId;
            existing.Updated_At = now;

            existing.StatusId = ReorderRatio.Ratio(existing.Quantity, existing.ReorderPoint);

            _auditLog.Log(AuditModuleEnum.Inventory, AuditActionEnum.Update, message, existing.Id, now);

            await _inventory.SaveChanges();

            return;
        }

        public async Task MarkAsDamaged(InventoryDamagePostViewModel damaged)
        {
            await _damageValidator.EnsureValidAsync(damaged);

            var inventory = await _inventory.GetInventoryWithTracking(damaged.Id);

            if (inventory == null)
            {
                throw new NotFound("Inventory item not found");
            }

            var now = DateTime.UtcNow;

            if (damaged.DamagedType == 1)
            {
                if (inventory.Quantity < damaged.Quantity)
                {
                    throw new BadRequest("Quantity is greater than what you have in stocks");
                }

                var sub = inventory.Quantity - damaged.Quantity;

                inventory.Quantity = sub;
                inventory.Updated_By = _auditLog.CurrentUserId;
                inventory.Updated_At = now;
                inventory.StatusId = ReorderRatio.Ratio(sub, inventory.ReorderPoint);
            }

            var damn = new DamagedInventory
            {
                InventoryId = inventory.Id,
                Reason = damaged.Reason.ToLower(),
                Quantity = damaged.Quantity,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
            };

            if (damaged.DamagedType == 1)
            {
                _auditLog.Log(
                    AuditModuleEnum.Inventory,
                    AuditActionEnum.CurrentItemDamage,
                    $"Marked {damaged.Quantity} '{ItemLabel(inventory)}' as damaged (Current Item): {damn.Reason}",
                    inventory.Id,
                    now
                );
            }
            else
            {
                _auditLog.Log(
                    AuditModuleEnum.Inventory,
                    AuditActionEnum.ReturnItemDamage,
                    $"Recorded {damaged.Quantity} returned '{ItemLabel(inventory)}' as damaged (Return Item): {damn.Reason}",
                    inventory.Id,
                    now
                );
            }

            await _inventory.Save(damn);

            var transaction = new InventoryTransactionPostViewModel
            {
                Id = inventory.Id,
                Quantity = -damaged.Quantity,
                Label = damaged.DamagedType == 1 ? (int)InventoryLabelEnum.Damage : (int)InventoryLabelEnum.Return,
                StockLevel = inventory.Quantity,
            };

            await InventoryTransaction(transaction);

            return;
        }

        // "Nails at Main Warehouse" — a stock record is named by its product and warehouse
        private static string ItemLabel(Inventory inventory)
        {
            return $"{inventory.Product?.Name ?? "unknown product"} at {inventory.Warehouse?.Name ?? "no warehouse"}";
        }

        public async Task InventoryTransaction(InventoryTransactionPostViewModel transaction)
        {
            await _transactionValidator.EnsureValidAsync(transaction);

            var transac = new InventoryTransaction
            {
                InventoryId = transaction.Id,
                QuantityChanged = transaction.Quantity,
                StockLevel = transaction.StockLevel,
                InventoryLabelId = transaction.Label,
                Created_By = _auditLog.CurrentUserId,
                Created_At = DateTime.UtcNow,
                IsActive = true,
            };

            await _inventory.Save(transac);

            return;
        }

        public async Task<IEnumerable<InventoryStatusViewModel>> InventoryStatusCount()
        {
            var data = await _inventory.StatusCount();

            return data;
        }

        #endregion

        #region Warehouse
        public async Task<WarehousePageViewModel> GetWarehouses(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var wareHouses = await _inventory.GetWarehousesWithoutTracking(page, pageSize, name, filter, cancellation);
            var count = await _inventory.WarehouseTotalCount(name, cancellation);

            return new WarehousePageViewModel
            {
                Warehouses = wareHouses,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };
        }

        public async Task NewWareHouse(InventoryWareHousePostViewModel wareHouse)
        {
            await _warehousePostValidator.EnsureValidAsync(wareHouse);

            var now = DateTime.UtcNow;

            var wh = new Warehouse
            {
                Name = wareHouse.Name.ToLower(),
                Address = wareHouse.Address.ToLower(),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
            };

            await _inventory.Save(wh);

            _auditLog.Log(
                AuditModuleEnum.Warehouse,
                AuditActionEnum.Create,
                $"Added warehouse '{wh.Name}'",
                wh.Id,
                now
            );
            await _inventory.SaveChanges();

            return;
        }

        public async Task UpdateWareHouse(InventoryWareHouseUpdateViewModel wareHouse)
        {
            await _warehouseUpdateValidator.EnsureValidAsync(wareHouse);

            var wh = await _inventory.GetIndividualWareHouseWithTracking(wareHouse.Id);

            if (wh == null)
            {
                throw new NotFound("Warehouse not found");
            }

            var now = DateTime.UtcNow;
            var oldName = wh.Name;
            var changes = new List<string>();

            var newName = wareHouse.Name.ToLower();
            var newAddress = wareHouse.Address.ToLower();

            if (wh.Name != newName)
            {
                changes.Add($"name '{wh.Name}' → '{newName}'");
            }

            if (wh.Address != newAddress)
            {
                changes.Add($"address '{wh.Address}' → '{newAddress}'");
            }

            wh.Name = newName;
            wh.Address = newAddress;
            wh.Updated_By = _auditLog.CurrentUserId;
            wh.Updated_At = now;

            var message =
                changes.Count > 0
                    ? $"Updated warehouse '{oldName}': {string.Join(", ", changes)}"
                    : $"Updated warehouse '{oldName}' (no changes)";

            _auditLog.Log(AuditModuleEnum.Warehouse, AuditActionEnum.Update, message, wh.Id, now);

            await _inventory.SaveChanges();

            return;
        }

        public async Task DeleteWareHouse(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Warehouse is required");
            }

            var wareHouse = await _inventory.GetIndividualWareHouseWithTracking(id);

            if (wareHouse == null)
            {
                throw new NotFound("Warehouse not found");
            }

            var now = DateTime.UtcNow;

            wareHouse.IsActive = false;
            wareHouse.Deleted_By = _auditLog.CurrentUserId;
            wareHouse.Deleted_At = now;

            var inventories = await _inventory.GetInventoryWithWareHouseId(id);

            foreach (var inv in inventories)
            {
                inv.WarehouseId = null;
                inv.Updated_By = _auditLog.CurrentUserId;
                inv.Updated_At = now;
            }

            var message =
                inventories.Count > 0
                    ? $"Deleted warehouse '{wareHouse.Name}' ({inventories.Count} inventory item(s) unassigned)"
                    : $"Deleted warehouse '{wareHouse.Name}'";

            _auditLog.Log(AuditModuleEnum.Warehouse, AuditActionEnum.Delete, message, wareHouse.Id, now);

            await _inventory.SaveChanges();

            return;
        }
        #endregion

        #region InventoryLabel
        public async Task<IEnumerable<InventoryLabelViewModel>> GetInventoryLabels()
        {
            var label = await _inventory.GetLabelForInsert();

            var final = label.Select(i => new InventoryLabelViewModel { Id = i.Id, Type = i.Type });

            return final;
        }
        #endregion

        #region Damage Inventory
        public async Task<IEnumerable<InventoryDamageViewModel>> GetDamageInventory(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Inventory item is required");
            }

            var damaged = await _inventory.GetDamageInventoryWithoutTracking(id);

            var final = damaged.Select(i => new InventoryDamageViewModel
            {
                Reason = i.Reason,
                Quantity = i.Quantity,
                Created_At = i.Created_At,
            });

            return final;
        }
        #endregion

        #region Trasactions
        public async Task<IEnumerable<InventoryTransactionViewModle>> GetInventoryItemTransaction(int id)
        {
            var transac = await _inventory.GetItemTransactionWithoutTracking(id);

            var final = transac.Select(t => new InventoryTransactionViewModle
            {
                Quantity = t.QuantityChanged,
                Label = t.InventoryLabel.Type,
                Created_At = t.Created_At,
            });

            return final;
        }
        #endregion

        #region Movements Velocity

        public async Task<InventoryMovementVelocityPageViewModel> GetMovementVelocity(
            int cutOffDate,
            int page,
            int pageSize
        )
        {
            var inventory = await _inventory.GetMovementVelocityWithoutTracking(cutOffDate, page, pageSize);

            var total = await _inventory.GetMovementVelocityCount(cutOffDate);

            var result = new InventoryMovementVelocityPageViewModel
            {
                inventories = inventory.Select(i => new InventoryMovementVelocityViewModel
                {
                    InventoryId = i.InventoryId,
                    Name = i.Name,
                    Classification = i.Classification,
                    VelocityMetric = i.VelocityMetric,
                    Warehouse = i.Warehouse,
                }),
                PageCount = (int)Math.Ceiling(total / (double)pageSize),
                Rows = total,
            };

            return result;
        }
        #endregion
    }
}
