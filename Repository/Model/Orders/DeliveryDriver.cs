using ERP.Repository.Model;

namespace ERP.Repository.Model.Orders
{
    public class DeliveryDriver : BaseModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public ICollection<Order> Orders { get; set; } = [];
    }
}
