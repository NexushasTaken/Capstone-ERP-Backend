using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.ViewModel.Sales
{


    public class SaleTotalViewModel
    {
        public int Id { get; set; }
        public string? OrderType { get; set; }
        public string? OrderStatus { get; set; }
        public string? DriverName { get; set; }
        public string? CustomerName { get; set; }
        public string? PickUpAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        public IEnumerable<OrderViewModel> Orders { get; set; } = [];
        public decimal Total { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class SalesPageViewModel
    {
        public IEnumerable<SaleTotalViewModel> Sales { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }
}
