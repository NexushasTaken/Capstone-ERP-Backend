using ERP.Repository.Model.Orders;

namespace ERP.Repository.Model.Inventories
{
    // A stock change with no sale, return or purchase behind it, never edited.
    // Created_At = when it was adjusted, Created_By = who did it.
    public class StockAdjustment : CreatedModel
    {
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        // AdjustmentReasonEnum
        public int ReasonId { get; set; }

        // Signed: what was added (+) or taken out (-)
        public int Quantity { get; set; }
        public string? Note { get; set; }

        // Set when the units came back damaged from a customer return
        public int? SaleReturnId { get; set; }
        public SaleReturn? SaleReturn { get; set; }
    }
}
