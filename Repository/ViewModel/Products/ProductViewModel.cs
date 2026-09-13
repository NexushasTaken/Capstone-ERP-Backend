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
}
