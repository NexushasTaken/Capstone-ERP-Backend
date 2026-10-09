namespace ERP.Repository.ViewModel.Forecast
{
    // One product's units sold (or returned, negative) at one moment, read from the stock diary
    public class DemandEntryViewModel
    {
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Units { get; set; }
    }

    public class ForecastProductViewModel
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }

        // When the product was first stocked anywhere
        public DateTime? FirstStocked { get; set; }
        public int StockOnHand { get; set; }
    }

    // A row of the Demand Forecast & Restock Recommendations card
    public class DemandForecastViewModel
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public double LowDemand { get; set; }
        public double ExpectedDemand { get; set; }
        public double BusyDemand { get; set; }
        public int StockOnHand { get; set; }
        public double? WeeksLeft { get; set; }
        public DateOnly? RunsOutAround { get; set; }
        public int SuggestedOrder { get; set; }
        public int Method { get; set; }
        public int HistoryWeeks { get; set; }
        public double? AiErrorPercent { get; set; }
        public double? BaselineErrorPercent { get; set; }
    }

    // Backtest over every product the AI was tested on, weighted by units sold
    public class ForecastAccuracyViewModel
    {
        public double AiErrorPercent { get; set; }
        public double BaselineErrorPercent { get; set; }
        public int ProductsTested { get; set; }
    }

    public class ForecastPageViewModel
    {
        public ICollection<DemandForecastViewModel> ForecastResults { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
        public int NeedOrderCount { get; set; }
        public ForecastAccuracyViewModel? Accuracy { get; set; }
        public DateTime? GeneratedAt { get; set; }
    }

    public class DemandHistoryPointViewModel
    {
        public DateOnly WeekStart { get; set; }
        public double Demand { get; set; }
    }

    public class DemandForecastPointViewModel
    {
        public DateOnly WeekStart { get; set; }
        public double Low { get; set; }
        public double Expected { get; set; }
        public double BusyCase { get; set; }
    }

    // The demand chart of one product: recent weekly demand plus the stored forecast weeks
    public class DemandChartViewModel
    {
        public DemandForecastViewModel Product { get; set; } = new();
        public ICollection<DemandHistoryPointViewModel> History { get; set; } = [];
        public ICollection<DemandForecastPointViewModel> Forecast { get; set; } = [];
    }
}
