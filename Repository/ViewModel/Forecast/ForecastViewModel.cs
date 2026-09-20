namespace ERP.Repository.ViewModel.Forecast
{
    public class ForecastViewModel
    {
        public int InventoryId { get; set; }
        public string? Name { get; set; }
        public DateTime? Day { get; set; }
        public int NetChange { get; set; }
        public int EndDayStock { get; set; }
    }

    public class DemandData
    {
        public float EndDayStock { get; set; }
    }

    public class ForeCastResultViewModel
    {
        public float[] Forecast { get; set; }
    }

    public class FinalForecastViewModel
    {
        public int InventoryId { get; set; }
        public string? Name { get; set; }
        public DateTime? EarliestStockOutDay { get; set; }
        public double ProbabilityNext30Days { get; set; }
        public float Stock { get; set; }
    }
}
