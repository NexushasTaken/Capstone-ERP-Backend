using ERP.Repository.Model;

namespace ERP.Repository.Model.Orders
{
    public class OrderStatus : KeyModel
    {
        public string? Status { get; set; }

        public ICollection<Order> Orders { get; set; } = [];
    }
}
