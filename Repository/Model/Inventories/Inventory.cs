namespace ERP.Repository.Model.Inventories
{
    public class Inventory : BaseModel
    {
        public int Name { get; set; }
        public int Quantity { get; set; }
        public int StatusId { get; set; }
        public InventoryStatus? InventoryStatus { get; set; }
        public int WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        public DateTime DateArrived { get; set; }

        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];
    }
}
