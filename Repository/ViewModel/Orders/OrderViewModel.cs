namespace ERP.Repository.ViewModel.Orders
{

    public enum OrdersFilter
    {
        PRDATOZ = 1,
        PRDZTOA = 2,
        QHIGH = 3,
        QLOW = 4,
    }
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? OrderType { get; set; }
        public string? OrderStatus { get; set; }
        public string? DriverName { get; set; }
        public int Quantity { get; set; }
        public string? CustomerName { get; set; }
        public string? PickUpAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public decimal Amount { get; set; }
        public string? BundleCode { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class OrderTotalViewModel
    {
        public IEnumerable<OrderViewModel> Orders { get; set; } = [];
        public decimal Total { get; set; }
    }

    public class OrderPageViewModel
    {
        public IEnumerable<OrderTotalViewModel> Orders { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class OrderPostViewModel
    {
        public int ProductId { get; set; }
        public int OrderTypeId { get; set; }
        public int? DeliveryRiderId { get; set; }
        public int Quantity { get; set; }
        public string? CustomerName { get; set; }
        public string? PickUpAddress { get; set; }
        public string? DeliveryAddress { get; set; }
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
        public int ProductId { get; set; }
        public int OrderId { get; set; }
        public int OrderStatusId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderDeliveryRiderViewModel
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
