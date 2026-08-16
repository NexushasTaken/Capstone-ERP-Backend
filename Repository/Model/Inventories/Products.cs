namespace ERP.Repository.Model.Inventories
{
    public class Product : BaseModel
    {
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Name { get; set; }
        public int Price { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
        public ICollection<Orders> Orders { get; set; } = [];
    }
}
