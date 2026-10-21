using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Orders
{
    // Units of one product a customer brought back from a completed sale, never edited.
    // Created_At = when it was returned, Created_By = who processed it.
    public class SaleReturn : CreatedModel
    {
        public int SaleId { get; set; }
        public Sale? Sale { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public int Quantity { get; set; }

        // ReturnConditionEnum: good units go back to stock, damaged ones go back and out again as a damaged adjustment.
        public int ConditionId { get; set; }
        public string? Reason { get; set; }

        // Money given back: the units' share of the sale's total, so the order discount applies.
        public decimal Refund { get; set; }

        // The damaged adjustment that took damaged units back out of stock
        public StockAdjustment? Adjustment { get; set; }
    }
}
