namespace ERP.Repository.ViewModel.Purchases
{
    public class PurchaseLineViewModel
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal UnitCost { get; set; }

        // Quantity × UnitCost
        public decimal Total { get; set; }
    }

    public class PurchaseViewModel
    {
        public int Id { get; set; }
        public string? SupplierName { get; set; }
        public DateOnly OrderedOn { get; set; }
        public string? Note { get; set; }
        public int StatusId { get; set; }
        public string? Status { get; set; }
        public IEnumerable<PurchaseLineViewModel> Lines { get; set; } = [];

        // Cost of what was ordered
        public decimal Total { get; set; }

        // Cost of what arrived; null until received
        public decimal? ReceivedTotal { get; set; }

        // UTC
        public DateTime? Created_At { get; set; }
        public DateTime? ReceivedAt { get; set; }

        // Days from the order date to the day it was received (Philippine time); display only
        public int? LeadTimeDays { get; set; }
    }

    public class PurchasePageViewModel
    {
        public IEnumerable<PurchaseViewModel> Purchases { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class PurchaseStatusCountViewModel
    {
        public string? Status { get; set; }
        public int Count { get; set; }
    }

    public class PurchaseLinePostViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class PurchasePostViewModel
    {
        public string? SupplierName { get; set; }
        public DateOnly OrderedOn { get; set; }
        public string? Note { get; set; }
        public List<PurchaseLinePostViewModel> Lines { get; set; } = [];
    }

    public class PurchasePutViewModel : PurchasePostViewModel
    {
        public int Id { get; set; }
    }

    public class PurchaseReceiveLineViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    // Every line of the purchase, with how many arrived (0 up to the ordered quantity)
    public class PurchaseReceiveViewModel
    {
        public int Id { get; set; }
        public List<PurchaseReceiveLineViewModel> Lines { get; set; } = [];
    }
}
