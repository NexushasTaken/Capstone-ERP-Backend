using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.PurchaseData;
using ERP.Repository.Interface.Purchases;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Purchases;
using ERP.Repository.ViewModel.Purchases;
using FluentValidation;

namespace ERP.Repository.Services.Purchases
{
    // Stock bought from suppliers. Stock only changes when a purchase is received; a received purchase is
    // locked, and mistakes after that are fixed with a count-correction adjustment.
    public class PurchaseService(
        IPurchaseData _purchases,
        IAuditLogService _auditLog,
        IValidator<PurchasePostViewModel> _postValidator,
        IValidator<PurchasePutViewModel> _putValidator,
        IValidator<PurchaseReceiveViewModel> _receiveValidator
    ) : IPurchaseService
    {
        public async Task<PurchasePageViewModel> GetPurchases(
            int page,
            int pageSize,
            string? name,
            int statusId,
            int filter,
            CancellationToken cancellation
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var purchases = (
                await _purchases.GetPurchasesWithoutTracking(page, pageSize, name, statusId, filter, cancellation)
            ).ToList();
            var count = await _purchases.PurchaseCount(name, statusId, cancellation);

            foreach (var purchase in purchases)
            {
                if (purchase.ReceivedAt is DateTime received)
                {
                    var receivedOn = DateOnly.FromDateTime(received + DemandWeek.PhOffset);
                    purchase.LeadTimeDays = receivedOn.DayNumber - purchase.OrderedOn.DayNumber;
                }
            }

            return new PurchasePageViewModel
            {
                Purchases = purchases,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };
        }

        public async Task<IEnumerable<PurchaseStatusCountViewModel>> StatusCount()
        {
            return await _purchases.StatusCount();
        }

        public async Task InsertPurchase(PurchasePostViewModel purchase)
        {
            await _postValidator.EnsureValidAsync(purchase);
            await EnsureProductsExist(purchase.Lines);

            var now = DateTime.UtcNow;
            var userId = _auditLog.CurrentUserId;

            var header = new Purchase
            {
                SupplierName = Clean(purchase.SupplierName),
                OrderedOn = purchase.OrderedOn,
                Note = Clean(purchase.Note),
                PurchaseStatusId = (int)PurchaseStatusEnum.Ordered,
                Lines = NewLines(purchase.Lines),
                Created_By = userId,
                Created_At = now,
            };

            await _purchases.Save(header);

            _auditLog.Log(
                AuditModuleEnum.Purchase,
                AuditActionEnum.Create,
                $"Created purchase {Label(header)} from {SupplierLabel(header.SupplierName)} ({header.Lines.Count} item(s), ₱{Total(header):N2})",
                header.Id,
                now
            );
            await _purchases.SaveChanges();
        }

        public async Task UpdatePurchase(PurchasePutViewModel purchase)
        {
            await _putValidator.EnsureValidAsync(purchase);

            var header = await OrderedPurchase(purchase.Id, "edited");
            await EnsureProductsExist(purchase.Lines);

            var now = DateTime.UtcNow;
            var supplier = Clean(purchase.SupplierName);
            var note = Clean(purchase.Note);
            var changes = new List<string>();

            if (header.SupplierName != supplier)
            {
                changes.Add($"supplier {SupplierLabel(header.SupplierName)} → {SupplierLabel(supplier)}");
            }

            if (header.OrderedOn != purchase.OrderedOn)
            {
                changes.Add($"order date {header.OrderedOn:yyyy-MM-dd} → {purchase.OrderedOn:yyyy-MM-dd}");
            }

            if (header.Note != note)
            {
                changes.Add("note changed");
            }

            var oldTotal = Total(header);
            var linesChanged =
                header.Lines.Count != purchase.Lines.Count
                || header.Lines.Any(l =>
                    !purchase.Lines.Any(n =>
                        n.ProductId == l.ProductId && n.Quantity == l.Quantity && n.UnitCost == l.UnitCost
                    )
                );

            if (linesChanged)
            {
                foreach (var line in header.Lines.ToList())
                {
                    _purchases.Remove(line);
                }

                header.Lines = NewLines(purchase.Lines);
                changes.Add($"items changed ({header.Lines.Count} item(s), ₱{oldTotal:N2} → ₱{Total(header):N2})");
            }

            header.SupplierName = supplier;
            header.OrderedOn = purchase.OrderedOn;
            header.Note = note;
            header.Updated_By = _auditLog.CurrentUserId;
            header.Updated_At = now;

            var summary = changes.Count > 0 ? $": {string.Join(", ", changes)}" : " (no changes)";

            _auditLog.Log(
                AuditModuleEnum.Purchase,
                AuditActionEnum.Update,
                $"Edited purchase {Label(header)}{summary}",
                header.Id,
                now
            );

            await _purchases.SaveChanges();
        }

        public async Task CancelPurchase(int id)
        {
            var header = await OrderedPurchase(id, "cancelled");
            var now = DateTime.UtcNow;

            header.PurchaseStatusId = (int)PurchaseStatusEnum.Cancelled;
            header.Updated_By = _auditLog.CurrentUserId;
            header.Updated_At = now;

            _auditLog.Log(
                AuditModuleEnum.Purchase,
                AuditActionEnum.StatusChange,
                $"Cancelled purchase {Label(header)} from {SupplierLabel(header.SupplierName)}",
                header.Id,
                now
            );

            await _purchases.SaveChanges();
        }

        // One step: every line's received quantity (0 up to what was ordered) goes into stock as a "received"
        // movement. A product with no stock record gets one, with reorder point 0 for the owner to set.
        public async Task ReceivePurchase(PurchaseReceiveViewModel receive)
        {
            await _receiveValidator.EnsureValidAsync(receive);

            var header = await OrderedPurchase(receive.Id, "received");
            var lines = header.Lines.OrderBy(l => l.Id).ToList();
            var arrived = new Dictionary<int, int>();
            var errors = new Dictionary<string, string[]>();

            for (var i = 0; i < lines.Count; i++)
            {
                var sent = receive.Lines.Where(l => l.ProductId == lines[i].ProductId).ToList();

                if (sent.Count != 1)
                {
                    errors[$"lines.{i}.quantity"] = ["Enter how many arrived"];
                }
                else if (sent[0].Quantity > lines[i].Quantity)
                {
                    errors[$"lines.{i}.quantity"] = [$"Only {lines[i].Quantity} were ordered"];
                }
                else
                {
                    arrived[lines[i].ProductId] = sent[0].Quantity;
                }
            }

            if (receive.Lines.Any(l => lines.All(line => line.ProductId != l.ProductId)))
            {
                errors["lines"] = ["That product is not part of this purchase"];
            }

            if (errors.Count > 0)
            {
                throw new ValidationFailed(errors);
            }

            var now = DateTime.UtcNow;
            var userId = _auditLog.CurrentUserId;
            var inventories = await _purchases.GetActiveInventoriesByProductWithTracking(
                arrived.Where(a => a.Value > 0).Select(a => a.Key)
            );
            var created = new List<(Inventory inventory, string name)>();
            var received = new List<string>();

            await using var transaction = await _purchases.BeginTransaction();

            foreach (var line in lines)
            {
                var quantity = arrived[line.ProductId];
                var name = line.Product?.Name ?? "unknown product";
                line.ReceivedQuantity = quantity;

                if (quantity == 0)
                {
                    received.Add($"0 of {line.Quantity} '{name}'");
                    continue;
                }

                if (!inventories.TryGetValue(line.ProductId, out var inventory))
                {
                    inventory = new Inventory
                    {
                        ProductId = line.ProductId,
                        ReorderPoint = 0,
                        IsActive = true,
                        Created_By = userId,
                        Created_At = now,
                    };
                    line.Product!.Inventory.Add(inventory);
                    inventories[line.ProductId] = inventory;
                    created.Add((inventory, name));
                }

                inventory.Quantity += quantity;
                inventory.StatusId = ReorderRatio.Ratio(inventory.Quantity, inventory.ReorderPoint);
                inventory.Updated_By = userId;
                inventory.Updated_At = now;
                inventory.InventoryTransactions.Add(
                    new InventoryTransaction
                    {
                        QuantityChanged = quantity,
                        StockLevel = inventory.Quantity,
                        InventoryLabelId = (int)InventoryLabelEnum.Received,
                        Created_By = userId,
                        Created_At = now,
                    }
                );

                var unit = line.Product?.Unit?.Name is string u ? $" {u}" : "";
                var of = quantity < line.Quantity ? $" of {line.Quantity}" : "";
                received.Add($"{quantity}{of}{unit} '{name}'");
            }

            header.PurchaseStatusId = (int)PurchaseStatusEnum.Received;
            header.ReceivedAt = now;
            header.ReceivedBy = userId;
            header.Updated_By = userId;
            header.Updated_At = now;

            // the new stock records need their ids before they can be logged
            await _purchases.SaveChanges();

            foreach (var (inventory, name) in created)
            {
                _auditLog.Log(
                    AuditModuleEnum.Inventory,
                    AuditActionEnum.Create,
                    $"Added stock record for '{name}' from purchase {Label(header)} (reorder point 0, set it in Inventory)",
                    inventory.Id,
                    now
                );
            }

            _auditLog.Log(
                AuditModuleEnum.Purchase,
                AuditActionEnum.IncreaseStock,
                $"Received purchase {Label(header)} from {SupplierLabel(header.SupplierName)}: {string.Join(", ", received)}",
                header.Id,
                now
            );

            await _purchases.SaveChanges();
            await transaction.CommitAsync();
        }

        // Only an ordered purchase can be edited, cancelled or received.
        private async Task<Purchase> OrderedPurchase(int id, string action)
        {
            if (id <= 0)
            {
                throw new BadRequest("Purchase is required");
            }

            var header = await _purchases.GetPurchaseWithTracking(id);

            if (header == null)
            {
                throw new NotFound("Purchase not found");
            }

            if (header.PurchaseStatusId != (int)PurchaseStatusEnum.Ordered)
            {
                throw new BadRequest($"Only an ordered purchase can be {action}");
            }

            return header;
        }

        private async Task EnsureProductsExist(List<PurchaseLinePostViewModel> lines)
        {
            var names = await _purchases.GetActiveProductNames(lines.Select(l => l.ProductId));
            var errors = new Dictionary<string, string[]>();

            for (var i = 0; i < lines.Count; i++)
            {
                if (!names.ContainsKey(lines[i].ProductId))
                {
                    errors[$"lines.{i}.productId"] = ["Product not found"];
                }
            }

            if (errors.Count > 0)
            {
                throw new ValidationFailed(errors);
            }
        }

        private List<PurchaseLine> NewLines(List<PurchaseLinePostViewModel> lines) =>
            lines
                .Select(l => new PurchaseLine
                {
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                })
                .ToList();

        private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim().ToLower();

        private static decimal Total(Purchase purchase) => purchase.Lines.Sum(l => l.Quantity * l.UnitCost);

        private static string Label(Purchase purchase) => $"PO-{purchase.Id}";

        private static string SupplierLabel(string? supplier) => supplier == null ? "no supplier" : $"'{supplier}'";
    }
}
