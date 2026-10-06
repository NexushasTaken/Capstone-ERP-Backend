namespace ERP.Repository.ViewModel.Products
{
    public class CategoryUpdateViewModel
    {
        public int Id { get; set; }
        public string? Type { get; set; }
    }

    public class CategoryViewModel : CategoryUpdateViewModel
    {
        public DateTime? Created_At { get; set; }
    }

    public class CategoryPageViewModel
    {
        public IEnumerable<CategoryViewModel> Categories { get; set; }
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }
}
