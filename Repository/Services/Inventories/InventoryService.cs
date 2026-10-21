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
using ERP.Repository.Model.Products;
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
        IValidator<StockAdjustmentPostViewModel> _adjustmentValidator
    ) : IInventoryService
    {
        #region Inventory
        public async Task<InventoryPageViewModel> GetInventories(
            int page,
            int pageSize,
            string name,
            int filter,
            int statusId,
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
                listFilter,
                cancellationToken
            );

            var totalCount = await _inventory.InventoryCount(name, filter, statusId, listFilter);

            var inventory = inv.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = i.Product?.Name,
                Quantity = i.Quantity,
                ReorderPoint = i.ReorderPoint,
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
                    Stock = p.Inventory.Sum(i => i.Quantity),
                })
                .ToList();
        }

        public async Task InsertItem(InventoryPostViewModel inventory)
        {
            await _inventoryPostValidator.EnsureValidAsync(inventory);

            var duplicate = await _inventory.CheckExistingInventory(inventory.ProductId);

            if (duplicate)
            {
                throw new BadRequest("This product already has a stock record.");
            }

            var product = await _product.GetProductByIdWithoutTracking(inventory.ProductId);

            if (product == null)
            {
                throw new NotFound("Product not found");
            }

            await CreateOpeningStock(product, inventory.Quantity, inventory.ReorderPoint, DateTime.UtcNow);

            return;
        }

        // Saves a product's stock record with an opening-stock adjustment and its ledger row, and the audit entry.
        // Shared by InsertItem and the product import.
        public async Task CreateOpeningStock(Product product, int quantity, int reorderPoint, DateTime now)
        {
            var userId = _auditLog.CurrentUserId;
            var inv = new Inventory
            {
                Quantity = quantity,
                ProductId = product.Id,
                ReorderPoint = reorderPoint,
                StatusId = ReorderRatio.Ratio(quantity, reorderPoint),
                IsActive = true,
                Created_By = userId,
                Created_At = now,
            };

            // the movement history starts from the real quantity
            inv.StockAdjustments.Add(
                new StockAdjustment
                {
                    ReasonId = (int)AdjustmentReasonEnum.OpeningStock,
                    Quantity = quantity,
                    Created_By = userId,
                    Created_At = now,
                }
            );
            inv.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    QuantityChanged = quantity,
                    StockLevel = quantity,
                    InventoryLabelId = (int)InventoryLabelEnum.Adjustment,
                    Created_By = userId,
                    Created_At = now,
                }
            );

            await _inventory.Save(inv);

            _auditLog.Log(
                AuditModuleEnum.Inventory,
                AuditActionEnum.Create,
                $"Added {inv.Quantity} '{product.Name}'",
                inv.Id,
                now
            );
            await _inventory.SaveChanges();
        }

        // Stock changes with no sale, return or purchase behind them. The reason sets the direction;
        // a count correction takes the counted quantity and saves the difference.
        public async Task Adjust(StockAdjustmentPostViewModel adjustment)
        {
            await _adjustmentValidator.EnsureValidAsync(adjustment);

            var inventory = await _inventory.GetInventoryWithTracking(adjustment.Id);

            if (inventory == null)
            {
                throw new NotFound("Inventory item not found");
            }

            var reason = (AdjustmentReasonEnum)adjustment.ReasonId;
            var before = inventory.Quantity;
            var change = reason switch
            {
                AdjustmentReasonEnum.Damaged or AdjustmentReasonEnum.Lost => -adjustment.Quantity,
                AdjustmentReasonEnum.Found => adjustment.Quantity,
                _ => adjustment.Quantity - before,
            };

            if (before + change < 0)
            {
                throw new BadRequest($"Only {before} in stock");
            }

            if (change == 0)
            {
                throw new BadRequest(
                    $"The counted quantity matches the stock ({before}), so there is nothing to adjust"
                );
            }

            var now = DateTime.UtcNow;
            var userId = _auditLog.CurrentUserId;
            var note = string.IsNullOrWhiteSpace(adjustment.Note) ? null : adjustment.Note.Trim().ToLower();

            inventory.Quantity = before + change;
            inventory.StatusId = ReorderRatio.Ratio(inventory.Quantity, inventory.ReorderPoint);
            inventory.Updated_By = userId;
            inventory.Updated_At = now;

            inventory.StockAdjustments.Add(
                new StockAdjustment
                {
                    ReasonId = adjustment.ReasonId,
                    Quantity = change,
                    Note = note,
                    Created_By = userId,
                    Created_At = now,
                }
            );
            inventory.InventoryTransactions.Add(
                new InventoryTransaction
                {
                    QuantityChanged = change,
                    StockLevel = inventory.Quantity,
                    InventoryLabelId = (int)InventoryLabelEnum.Adjustment,
                    Created_By = userId,
                    Created_At = now,
                }
            );

            var counted =
                reason == AdjustmentReasonEnum.CountCorrection ? $", counted {inventory.Quantity} (was {before})" : "";

            _auditLog.Log(
                AuditModuleEnum.Inventory,
                AuditActionEnum.Adjust,
                $"Adjusted '{ItemLabel(inventory)}' by {change:+#;-#} ({AdjustmentReasons.Label(adjustment.ReasonId)}{counted})"
                    + (note == null ? "" : $": {note}"),
                inventory.Id,
                now
            );

            await _inventory.SaveChanges();
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

            // the product is fixed; only the reorder point changes
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

        // a stock record is named by its product
        private static string ItemLabel(Inventory inventory)
        {
            return inventory.Product?.Name ?? "unknown product";
        }

        public async Task<IEnumerable<InventoryStatusViewModel>> InventoryStatusCount()
        {
            var data = await _inventory.StatusCount();

            return data;
        }

        #endregion

        #region Adjustments
        public async Task<IEnumerable<StockAdjustmentViewModel>> GetAdjustments(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Inventory item is required");
            }

            var adjustments = await _inventory.GetAdjustmentsWithoutTracking(id);

            return adjustments.Select(a => new StockAdjustmentViewModel
            {
                ReasonId = a.ReasonId,
                Reason = AdjustmentReasons.Label(a.ReasonId),
                Quantity = a.Quantity,
                Note = a.Note,
                SaleId = a.SaleReturn?.SaleId,
                Created_At = a.Created_At,
            });
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
    }
}
