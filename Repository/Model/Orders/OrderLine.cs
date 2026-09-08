using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Orders
{
    public class OrderLine : BaseModel
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
