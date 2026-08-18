using ERP.Repository.Model;

namespace ERP.Repository.Model.Orders
{
    public class OrderStatus : BaseModel
    {
        public string? Status { get; set; }

        public ICollection<Order> Orders { get; set; } = [];
    }
}
