namespace ERP.Repository.Model.Purchases
{
    public class PurchaseStatus : KeyModel
    {
        public string? Status { get; set; }

        public ICollection<Purchase> Purchases { get; set; } = [];
    }
}
