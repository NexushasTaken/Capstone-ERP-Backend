using ERP.Repository.Model;

namespace ERP.Repository.Model.Orders
{
    public class Discount : KeyModel
    {
        public string? Type { get; set; }

        // The fixed legal rate; null for custom, whose rate is typed on the order.
        public decimal? Percent { get; set; }

        public ICollection<Order> Orders { get; set; } = [];
    }
}
