using ERP.Repository.Model;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Inventories
{
    public class Inventory : BaseModel
    {
        public int Quantity { get; set; }
        public int StatusId { get; set; }
        public InventoryStatus? InventoryStatus { get; set; }
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public int ReorderPoint { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];
        public ICollection<DamagedInventory> DamagedInventories { get; set; } = [];
        public ICollection<OrderLine> OrderLines { get; set; } = [];
    }
}
