using ERP.Repository.Model.Inventories;

namespace ERP.Repository.Model.Forecast
{
    public class ForecastResult : BaseModel
    {
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; }
        public DateTime? EarliestStockOutDay { get; set; }
    }
}
