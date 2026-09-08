using ERP.Repository.Model;
using ERP.Repository.Model.Products;
using ERP.Repository.Model.Sales;

namespace ERP.Repository.Model.Orders
{
    public class Order : BaseModel
    {
        public int OrderTypeId { get; set; }
        public OrderType? OrderType { get; set; }
        public int OrderStatusId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public int? DeliveryDriverId { get; set; }
        public DeliveryDriver? DeliveryDriver { get; set; }
        public string? CustomerName { get; set; }
        public string? PibkupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<Sale> Sales { get; set; } = [];
        public ICollection<OrderLine> OrderLines { get; set; } = [];
    }
}
