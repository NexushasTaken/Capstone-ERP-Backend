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

    public class InventoryRestockViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }

    public class InventoryUpdateViewModel
    {
        public int Id { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class InventoryDamageViewModel
    {
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryDamagePostViewModel : InventoryDamageViewModel
    {
        public int Id { get; set; }
    }

    public class InventoryTransactionViewModle
    {
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryTransactionPostViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Label { get; set; }

        // stock on hand after this movement; the forecast reads it
        public int StockLevel { get; set; }
    }

    public class InventoryLabelViewModel
    {
        public int Id { get; set; }
        public string? Type { get; set; }
    }
}
