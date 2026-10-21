namespace ERP.Repository.ViewModel.Inventories
{
    public enum InventoryFilter
    {
        ATOZ = 1,
        ZTOA = 2,
        QHIGH = 3,
        QLOW = 4,
        RPOINT = 5,
    }

    // Optional list filters; zero / null means "not filtered".
    public record InventoryListFilter(int CategoryId = 0, int? MinQuantity = null, int? MaxQuantity = null);

    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Name { get; set; }

        // The product's unit of measurement, e.g. "pc"
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public int ReorderPoint { get; set; }
        public string? Status { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryPageViewModel
    {
        public IEnumerable<InventoryViewModel> inventories { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class InventoryStatusViewModel()
    {
        public string? Status { get; set; }
        public int Count { get; set; }
    }

    public class InventoryPostViewModel
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class InventoryUpdateViewModel
    {
        public int Id { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class StockAdjustmentPostViewModel
    {
        // Inventory Id
        public int Id { get; set; }

        // AdjustmentReasonEnum 1-4; opening stock is written by the system
        public int ReasonId { get; set; }

        // Units damaged, lost or found; for a count correction, the counted quantity
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }

    public class StockAdjustmentViewModel
    {
        public int ReasonId { get; set; }
        public string? Reason { get; set; }

        // Signed: + added, - taken out
        public int Quantity { get; set; }
        public string? Note { get; set; }

        // The sale the units came back from, for a damaged customer return
        public int? SaleId { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryTransactionViewModle
    {
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public DateTime? Created_At { get; set; }
    }
}
