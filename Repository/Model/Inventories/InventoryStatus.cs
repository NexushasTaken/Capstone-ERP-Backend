namespace ERP.Repository.Model.Inventories
{
    public class InventoryStatus : BaseModel
    {
        public string? Status { get; set; }
        public Inventory? Inventory { get; set; }
    }
}
