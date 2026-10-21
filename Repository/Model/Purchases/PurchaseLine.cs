using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Purchases
{
    // One product of a purchase. Replaced (hard-deleted) when an ordered purchase is edited.
    public class PurchaseLine : KeyModel
    {
        public int PurchaseId { get; set; }
        public Purchase? Purchase { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // Ordered quantity, in the product's unit
        public int Quantity { get; set; }

        // What arrived; set when the purchase is received, at most Quantity
        public int? ReceivedQuantity { get; set; }

        // What the store pays per unit, separate from the selling price
        public decimal UnitCost { get; set; }
    }
}
