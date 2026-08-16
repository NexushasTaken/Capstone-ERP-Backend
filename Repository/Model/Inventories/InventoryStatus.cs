namespace ERP.Repository.Model.Inventories
{
    public class InventoryStatus : BaseModel
    {
        public string? Status { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = [];
    }
}
