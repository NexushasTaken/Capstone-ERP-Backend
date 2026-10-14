namespace ERP.Repository.Model.Orders
{
    // Written once when an order is completed (a terminal status), never edited.
    // Created_At = when it was sold, Created_By = who completed the order.
    public class Sale : CreatedModel
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        // Amount charged, net of the order's discount, at the time of the sale.
        public decimal Total { get; set; }
    }
}
