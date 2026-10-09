using ERP.Repository.Model.Products;

namespace ERP.Repository.Model.Forecast
{
    // One product's demand forecast for the next 4 weeks and what to order.
    // Every forecast run replaces all rows.
    public class ForecastResult : BaseModel
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // 4-week totals of forecasted demand (units sold - units returned)
        public double LowDemand { get; set; }
        public double ExpectedDemand { get; set; }
        public double BusyDemand { get; set; }

        public int StockOnHand { get; set; }
        public double? WeeksLeft { get; set; }
        public DateOnly? RunsOutAround { get; set; }
        public int SuggestedOrder { get; set; }

        // ForecastMethodEnum
        public int Method { get; set; }
        public int HistoryWeeks { get; set; }

        // Backtest on the last 12 weeks; null when the AI wasn't tested.
        // Raw sums, so the overall error % can be weighted by units sold.
        public double? BacktestSold { get; set; }
        public double? AiAbsError { get; set; }
        public double? BaselineAbsError { get; set; }

        public ICollection<ForecastWeek> Weeks { get; set; } = [];
    }
}
