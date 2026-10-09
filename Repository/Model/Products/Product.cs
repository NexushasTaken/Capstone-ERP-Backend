using ERP.Repository.Model.Forecast;
using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Orders;

namespace ERP.Repository.Model.Products
{
    public class Product : BaseModel
    {
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }

        public ICollection<Inventory> Inventory { get; set; } = [];
        public ICollection<OrderLine> OrderLines { get; set; } = [];
        public ICollection<ForecastResult> ForecastResults { get; set; } = [];
    }
}
