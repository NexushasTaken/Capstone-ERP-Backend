namespace ERP.Repository.Model.Inventories
{
    public class Category : BaseModel
    {
        public string? Type { get; set; }

        public ICollection<Product> Products { get; set; } = [];
    }
}
