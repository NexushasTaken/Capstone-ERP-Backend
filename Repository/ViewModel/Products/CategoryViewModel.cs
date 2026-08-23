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
}
