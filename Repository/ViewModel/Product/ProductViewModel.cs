namespace ERP.Repository.ViewModel.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }
        public DateTime? Created_At { get; set; }
    }
}
