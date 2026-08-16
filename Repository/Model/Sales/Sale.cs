using ERP.Repository.Model.Orders;

namespace ERP.Repository.Model.Sales
{
    public class Sale : BaseModel
    {
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int TotalAmount { get; set; }
    }
}
