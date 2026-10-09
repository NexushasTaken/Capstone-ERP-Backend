namespace ERP.Repository.Model.Forecast
{
    // One of the 4 forecast weeks behind a ForecastResult, drawn by the demand chart.
    public class ForecastWeek : BaseModel
    {
        public int ForecastResultId { get; set; }
        public ForecastResult? ForecastResult { get; set; }

        // Monday of the week, in Philippine time
        public DateOnly WeekStart { get; set; }
        public double Low { get; set; }
        public double Expected { get; set; }
        public double BusyCase { get; set; }
    }
}
