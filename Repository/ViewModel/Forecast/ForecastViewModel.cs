namespace ERP.Repository.ViewModel.Forecast
{
    public class ForecastViewModel
    {
        public int InventoryId { get; set; }
        public DateTime? Day { get; set; }
        public int NetChange { get; set; }
        public int EndDayStock { get; set; }
    }

    public class DemandData
    {
        public float NetChange { get; set; }
    }

    public class ForeCastResultViewModel
    {
        public float[] Forecast { get; set; }
    }

    public class FinalForecastViewModel
    {
        public int InventoryId { get; set; }
        public DateTime Day { get; set; }
        public float Stock { get; set; }
    }
}
