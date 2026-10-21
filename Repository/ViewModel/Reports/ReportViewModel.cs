using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.ViewModel.Reports
{
    // Who and when, printed in every report's header. GeneratedAt is Philippine time.
    public class ReportHeader
    {
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "";
    }

    public class StockReportRow
    {
        public int InventoryId { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public int ReorderPoint { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
    }

    // Stock on hand as of GeneratedAt; rows are critical first, then low stock, then available.
    public class StockReportViewModel : ReportHeader
    {
        public List<StockReportRow> Rows { get; set; } = [];
    }

    public class PurchaseReportLine
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }
        public int Ordered { get; set; }
        public int Received { get; set; }
        public decimal Cost { get; set; }
    }

    public class PurchaseReportRow
    {
        public int Id { get; set; }
        public DateOnly OrderedOn { get; set; }
        public DateOnly ReceivedOn { get; set; }
        public string? SupplierName { get; set; }
        public decimal OrderedTotal { get; set; }

        // Cost of what arrived
        public decimal ReceivedTotal { get; set; }
        public List<PurchaseReportLine> Lines { get; set; } = [];
    }

    // Purchases received in the period (by received day).
    public class PurchaseReportViewModel : ReportHeader
    {
        public SaleReportPeriod Period { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public List<PurchaseReportRow> Purchases { get; set; } = [];
    }

    public class AdjustmentReportRow
    {
        // UTC
        public DateTime? Created_At { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }
        public int ReasonId { get; set; }

        // Signed
        public int Quantity { get; set; }
        public string? Note { get; set; }

        // Set for damaged customer returns
        public int? SaleId { get; set; }
        public string? By { get; set; }
    }

    // Stock adjustments made in the period.
    public class AdjustmentReportViewModel : ReportHeader
    {
        public SaleReportPeriod Period { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public List<AdjustmentReportRow> Rows { get; set; } = [];
    }

    public record ReportFile(byte[] Content, string FileName);
}
