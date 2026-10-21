using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;
using ERP.Repository.Model.Purchases;

namespace ERP.Repository.Model.Products
{
    public class Product : BaseModel
    {
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }

        // Required; defaults to pc (Unit.PieceId)
        public int UnitId { get; set; } = Unit.PieceId;
        public Unit? Unit { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
        public ICollection<OrderLine> OrderLines { get; set; } = [];
        public ICollection<PurchaseLine> PurchaseLines { get; set; } = [];
        public ICollection<ForecastResult> ForecastResults { get; set; } = [];
    }
}
