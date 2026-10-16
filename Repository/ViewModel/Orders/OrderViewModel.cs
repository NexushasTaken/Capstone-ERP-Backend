using ERP.Repository.Configuration.Helper;

namespace ERP.Repository.ViewModel.Orders
{
    public enum OrdersFilter
    {
        PRDATOZ = 1,
        PRDZTOA = 2,
        QLOW = 3,
        QHIGH = 4,
    }

    public class OrderViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderTotalViewModel
    {
        public int OrderId { get; set; }
        public string? OrderType { get; set; }
        public string? OrderStatus { get; set; }
        public int? DeliveryDriverId { get; set; }
        public string? DriverName { get; set; }
        public string? CustomerName { get; set; }
        public string? DeliveryAddress { get; set; }
        public IEnumerable<OrderViewModel> Orders { get; set; } = [];
        public decimal Subtotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? DiscountType { get; set; }
        public string? DiscountIdNumber { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public DateTime? Created_At { get; set; }
        public DateTime? Updated_At { get; set; }
    }

    public class OrderPageViewModel
    {
        public IEnumerable<OrderTotalViewModel> Orders { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class OrderStatusCount
    {
        public string? Status { get; set; }
        public int Count { get; set; }
    }

    public class OrderLineViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public interface IOrderDiscount
    {
        decimal DiscountPercent { get; }
        string DiscountType { get; }
        string? DiscountIdNumber { get; }
    }

    public class OrderPostViewModel : IOrderDiscount
    {
        public int OrderTypeId { get; set; }
        public int? DeliveryDriverId { get; set; }
        public string? CustomerName { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal DiscountPercent { get; set; }
        public string DiscountType { get; set; } = DiscountTypes.None;
        public string? DiscountIdNumber { get; set; }
        public ICollection<OrderLineViewModel> OrderLines { get; set; } = [];
    }

    public class OrderPutViewModel : IOrderDiscount
    {
        public int OrderId { get; set; }
        public int? DeliveryDriverId { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal DiscountPercent { get; set; }
        public string DiscountType { get; set; } = DiscountTypes.None;
        public string? DiscountIdNumber { get; set; }
        public ICollection<OrderLineViewModel> OrderLines { get; set; } = [];
    }

    public class OrderTypeViewModel
    {
        public int Id { get; set; }
        public string? Type { get; set; }
    }

    public class OrderStatusViewModel
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }

    public class OrderStatusPostViewModel
    {
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
    }

    public class OrderDeliveryDriverViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
