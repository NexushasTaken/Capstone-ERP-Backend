using ERP.Repository.Model;

namespace ERP.Repository.Model.Orders
{
    public class OrderType : KeyModel
    {
        public string? Type { get; set; }

        public ICollection<Order> Orders { get; set; } = [];
    }
}
