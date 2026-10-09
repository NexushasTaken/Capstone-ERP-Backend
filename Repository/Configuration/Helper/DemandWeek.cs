namespace ERP.Repository.Configuration.Helper
{
    // Demand is counted per week, Monday to Sunday, in Philippine time (UTC+8, no DST).
    public static class DemandWeek
    {
        public static readonly TimeSpan PhOffset = TimeSpan.FromHours(8);

        public static DateOnly PhToday()
        {
            return DateOnly.FromDateTime(DateTime.UtcNow + PhOffset);
        }

        // Monday of the week the PH date falls in
        public static DateOnly StartOf(DateOnly day)
        {
            int sinceMonday = ((int)day.DayOfWeek + 6) % 7;
            return day.AddDays(-sinceMonday);
        }

        // Monday of the week the UTC time falls in, in PH time
        public static DateOnly StartOf(DateTime utc)
        {
            return StartOf(DateOnly.FromDateTime(utc + PhOffset));
        }
    }
}
