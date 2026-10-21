using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.ReportData;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Purchases;
using ERP.Repository.Model.UserAccounts;
using ERP.Repository.ViewModel.Reports;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ReportData
{
    public class ReportData(DatabaseContext _context) : BaseData(_context), IReportData
    {
        // Every active stock record: critical, then low stock, then available; A to Z within each.
        public async Task<List<StockReportRow>> StockRows(CancellationToken cancellation = default)
        {
            return await BaseQuery<Inventory>(false)
                .Where(i => i.IsActive)
                .OrderByDescending(i => i.StatusId)
                .ThenBy(i => i.Product!.Name)
                .ThenBy(i => i.Id)
                .Select(i => new StockReportRow
                {
                    InventoryId = i.Id,
                    ProductName = i.Product!.Name,
                    Category = i.Product.Category == null ? null : i.Product.Category.Type,
                    Unit = i.Product.Unit!.Name,
                    Quantity = i.Quantity,
                    ReorderPoint = i.ReorderPoint,
                    StatusId = i.StatusId,
                    Status = i.InventoryStatus!.Status,
                })
                .ToListAsync(cancellation);
        }

        // Purchases received in [start, end) (UTC), oldest first.
        public async Task<List<PurchaseReportRow>> ReceivedPurchases(
            DateTime start,
            DateTime end,
            CancellationToken cancellation = default
        )
        {
            var rows = await BaseQuery<Purchase>(false)
                .Where(p => p.ReceivedAt != null && p.ReceivedAt >= start && p.ReceivedAt < end)
                .OrderBy(p => p.ReceivedAt)
                .ThenBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.OrderedOn,
                    p.ReceivedAt,
                    p.SupplierName,
                    Lines = p
                        .Lines.OrderBy(l => l.Id)
                        .Select(l => new PurchaseReportLine
                        {
                            ProductId = l.ProductId,
                            ProductName = l.Product!.Name,
                            Unit = l.Product.Unit!.Name,
                            Ordered = l.Quantity,
                            Received = l.ReceivedQuantity ?? 0,
                            Cost = (l.ReceivedQuantity ?? 0) * l.UnitCost,
                        })
                        .ToList(),
                    OrderedTotal = p.Lines.Sum(l => l.Quantity * l.UnitCost),
                })
                .ToListAsync(cancellation);

            return rows.Select(p => new PurchaseReportRow
                {
                    Id = p.Id,
                    OrderedOn = p.OrderedOn,
                    ReceivedOn = DateOnly.FromDateTime(p.ReceivedAt!.Value + DemandWeek.PhOffset),
                    SupplierName = p.SupplierName,
                    OrderedTotal = p.OrderedTotal,
                    ReceivedTotal = p.Lines.Sum(l => l.Cost),
                    Lines = p.Lines,
                })
                .ToList();
        }

        // Adjustments made in [start, end) (UTC), oldest first, with who made them.
        public async Task<List<AdjustmentReportRow>> Adjustments(
            DateTime start,
            DateTime end,
            CancellationToken cancellation = default
        )
        {
            var users = BaseQuery<UserAccount>(false);

            return await BaseQuery<StockAdjustment>(false)
                .Where(a => a.Created_At >= start && a.Created_At < end)
                .OrderBy(a => a.Created_At)
                .ThenBy(a => a.Id)
                .Select(a => new AdjustmentReportRow
                {
                    Created_At = a.Created_At,
                    ProductName = a.Inventory!.Product!.Name,
                    Unit = a.Inventory.Product.Unit!.Name,
                    ReasonId = a.ReasonId,
                    Quantity = a.Quantity,
                    Note = a.Note,
                    SaleId = a.SaleReturn == null ? null : a.SaleReturn.SaleId,
                    By = users
                        .Where(u => u.Id == a.Created_By)
                        .Select(u => string.Concat(u.UserInformation!.FirstName, " ", u.UserInformation.LastName))
                        .FirstOrDefault(),
                })
                .ToListAsync(cancellation);
        }
    }
}
