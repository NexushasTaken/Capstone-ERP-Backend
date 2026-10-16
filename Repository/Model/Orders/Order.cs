using ERP.Repository.Configuration.Helper;
using ERP.Repository.Model;
using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Orders
{
    public class Order : UpdatedModel
    {
        public int OrderTypeId { get; set; }
        public OrderType? OrderType { get; set; }
        public int OrderStatusId { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public int? DeliveryDriverId { get; set; }
        public DeliveryDriver? DeliveryDriver { get; set; }
        public string? CustomerName { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal DiscountPercent { get; set; }

        // One of DiscountTypes; the ID number is only kept for the legal (preset) discounts.
        public string DiscountType { get; set; } = DiscountTypes.None;
        public string? DiscountIdNumber { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; } = [];
        public Sale? Sale { get; set; }
    }
}
