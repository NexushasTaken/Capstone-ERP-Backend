namespace ERP.Repository.Model.Purchases
{
    // Stock the store buys from a supplier. Ordered → received (stock goes up) or ordered → cancelled.
    // Only an ordered purchase can be edited; Created_At = when it was recorded.
    public class Purchase : UpdatedModel
    {
        // Optional free text, like Order.CustomerName; there is no supplier list
        public string? SupplierName { get; set; }

        // The day the order was placed with the supplier (Philippine date)
        public DateOnly OrderedOn { get; set; }
        public string? Note { get; set; }

        // PurchaseStatusEnum
        public int PurchaseStatusId { get; set; }
        public PurchaseStatus? PurchaseStatus { get; set; }

        // Set when it is received
        public DateTime? ReceivedAt { get; set; }
        public int? ReceivedBy { get; set; }

        public ICollection<PurchaseLine> Lines { get; set; } = [];
    }
}
