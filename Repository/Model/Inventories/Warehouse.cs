using ERP.Repository.Model;

namespace ERP.Repository.Model.Inventories
{
    public class Warehouse : BaseModel
    {
        public string? Name { get; set; }
        public string? Address { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
    }
}
