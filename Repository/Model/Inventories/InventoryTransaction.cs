using ERP.Repository.Model;

namespace ERP.Repository.Model.Inventories
{
    public class InventoryTransaction : BaseModel
    {
        public int InventoryId  { get; set; }
        public Inventory? Inventory { get; set; }
        public int QuantityChanged { get; set; }
        public int InventoryLabelId { get; set; }
        public InventoryLabel? InventoryLabel { get; set; }
    }
}
