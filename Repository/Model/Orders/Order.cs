using ERP.Repository.Configuration.Enum;
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

        public int DiscountId { get; set; } = (int)DiscountTypeEnum.None;
        public Discount? Discount { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; } = [];
        public Sale? Sale { get; set; }
    }
}
