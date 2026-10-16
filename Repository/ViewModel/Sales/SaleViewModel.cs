using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.ViewModel.Sales
{
    public class SaleTotalViewModel
    {
        public int Id { get; set; }
        public string? OrderType { get; set; }
        public string? OrderStatus { get; set; }
        public string? DriverName { get; set; }
        public string? CustomerName { get; set; }
        public string? DeliveryAddress { get; set; }
        public IEnumerable<SaleLineViewModel> Orders { get; set; } = [];
        public decimal Subtotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }

        // Sum of the refunds of this sale's returns, and what the sale is worth after them.
        public decimal Refunded { get; set; }
        public decimal TotalAfterRefunds { get; set; }
        public DateTime? Created_At { get; set; }
    }

    // One product of a sale (its order lines added up), with how much of it has come back.
    public class SaleLineViewModel : OrderViewModel
    {
        public int ReturnedQuantity { get; set; }

        // Price per unit when it sold (line amounts / quantity), before the order discount.
        public decimal SoldUnitPrice { get; set; }
    }

    // A return of one or more products of a completed sale.
    public class SaleReturnPostViewModel
    {
        public int SaleId { get; set; }
        public string? Reason { get; set; }
        public List<SaleReturnItemViewModel> Items { get; set; } = [];
    }

    public class SaleReturnItemViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // ReturnConditionEnum: 1 good, 2 damaged
        public int ConditionId { get; set; }
    }

    // One row of the Returns list; Created_At is UTC.
    public class SaleReturnViewModel
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public string? ProductName { get; set; }
        public string? CustomerName { get; set; }
        public int Quantity { get; set; }
        public string Condition { get; set; } = "";
        public decimal Refund { get; set; }
        public string? Reason { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class SaleReturnPageViewModel
    {
        public IEnumerable<SaleReturnViewModel> Returns { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class SalesPageViewModel
    {
        public IEnumerable<SaleTotalViewModel> Sales { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }

        // Sum of TotalAfterRefunds over every sale matching the filters, not just this page.
        public decimal TotalAmount { get; set; }
    }

    // First and last day (Philippine time) that has a sale; both null when there are no sales.
    public class SaleDateRangeViewModel
    {
        public DateOnly? First { get; set; }
        public DateOnly? Last { get; set; }
    }

    public enum SaleReportPeriod
    {
        Daily,
        Weekly,
        Monthly,
    }

    // One sale as the PDF report reads it; Created_At is UTC.
    public class SaleReportSale
    {
        public int Id { get; set; }
        public DateTime? Created_At { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderType { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal Refunded { get; set; }
        public List<SaleReportLine> Lines { get; set; } = [];
    }

    public class SaleReportLine
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    // One day (weekly report) or one week (monthly report).
    public class SaleReportRow
    {
        public string Label { get; set; } = "";
        public int Sales { get; set; }
        public decimal Gross { get; set; }
        public decimal Discount { get; set; }
        public decimal Net { get; set; }
        public decimal Refunded { get; set; }
        public decimal AfterRefunds { get; set; }
    }

    public class SaleReportProduct
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }

        // Sum of line amounts, before the order discount.
        public decimal Revenue { get; set; }
    }

    // Everything the PDF shows. From/To are inclusive Philippine days; GeneratedAt is Philippine time.
    public class SaleReportViewModel
    {
        public SaleReportPeriod Period { get; set; }
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = "";
        public int SalesCount { get; set; }
        public decimal Gross { get; set; }
        public decimal Discount { get; set; }
        public decimal Net { get; set; }

        // Refunds of the period's sales (attributed to the sale's day, like the Sales list).
        public decimal Refunded { get; set; }
        public decimal AfterRefunds { get; set; }
        public decimal Average { get; set; }

        // Daily report: every sale of the day. Weekly / monthly: Rows instead.
        public IList<SaleReportSale> Sales { get; set; } = [];
        public IList<SaleReportRow> Rows { get; set; } = [];
        public IList<SaleReportProduct> TopProducts { get; set; } = [];
    }

    public record SaleReportFile(byte[] Content, string FileName);
}
