namespace ERP.Repository.Model.Inventories
{
    public class DamagedInventory : BaseModel
    {
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public string? Reason { get; set; }
        public int Quantity { get; set; }
    }
}
