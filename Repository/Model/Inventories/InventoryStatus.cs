using ERP.Repository.Model;

namespace ERP.Repository.Model.Inventories
{
    public class InventoryStatus : KeyModel
    {
        public string? Status { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = [];
    }
}
