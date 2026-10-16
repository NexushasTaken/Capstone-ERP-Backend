namespace ERP.Repository.ViewModel.Products
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? Created_At { get; set; }
        public string? CreatedByName { get; set; }
        public DateTime? Updated_At { get; set; }
        public string? UpdatedByName { get; set; }

        /// <summary>Stock on hand; only set by the inventory/order product picker.</summary>
        public int? Stock { get; set; }
    }

    public class ProductPageViewModel()
    {
        public IEnumerable<ProductViewModel> Products { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class ProductPostViewModel
    {
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductUpdateViewModel : ProductPostViewModel
    {
        public int Id { get; set; }
    }

    public class ProductImportRow
    {
        public string? Name { get; set; }

        /// <summary>Category name, matched case-insensitively; blank means uncategorized.</summary>
        public string? Category { get; set; }
        public decimal Price { get; set; }

        /// <summary>Opening stock; when set, a stock record is created with ReorderPoint.</summary>
        public int? Quantity { get; set; }
        public int? ReorderPoint { get; set; }
    }

    public class ProductImportViewModel
    {
        public List<ProductImportRow> Rows { get; set; } = [];
    }
}
