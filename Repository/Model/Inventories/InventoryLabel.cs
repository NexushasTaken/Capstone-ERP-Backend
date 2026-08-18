using ERP.Repository.Model;

namespace ERP.Repository.Model.Inventories
{
    public class InventoryLabel : BaseModel
    {
        public string? Type { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = [];
    }
}
