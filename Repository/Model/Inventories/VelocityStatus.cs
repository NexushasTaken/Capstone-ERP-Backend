using ERP.Repository.Model;

namespace ERP.Repository.Model.Inventories
{
    public class VelocityStatus : BaseModel
    {
        public string? Status { get; set; }
        public ICollection<Inventory> Inventory { get; set; } = [];
    }
}
