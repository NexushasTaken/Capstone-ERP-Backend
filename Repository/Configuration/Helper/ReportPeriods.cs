using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.Configuration.Helper
{
    // The daily / weekly / monthly periods every period report uses (Philippine days).
    public static class ReportPeriods
    {
        public static SaleReportPeriod Parse(string? period) =>
            period?.Trim().ToLowerInvariant() switch
            {
                "daily" => SaleReportPeriod.Daily,
                "weekly" => SaleReportPeriod.Weekly,
                "monthly" => SaleReportPeriod.Monthly,
                _ => throw new BadRequest("The period must be daily, weekly or monthly"),
            };

        // daily: the day; weekly: its Monday-to-Sunday week; monthly: its month. Both ends inclusive.
        public static (DateOnly From, DateOnly To) Range(SaleReportPeriod period, DateOnly date) =>
            period switch
            {
                SaleReportPeriod.Weekly => (DemandWeek.StartOf(date), DemandWeek.StartOf(date).AddDays(6)),
                SaleReportPeriod.Monthly => (
                    new DateOnly(date.Year, date.Month, 1),
                    new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1)
                ),
                _ => (date, date),
            };

        public static string Text(SaleReportPeriod period, DateOnly from, DateOnly to) =>
            period switch
            {
                SaleReportPeriod.Daily => $"{from:dddd, MMMM d, yyyy}",
                SaleReportPeriod.Monthly => $"{from:MMMM yyyy}",
                _ when from.Year == to.Year => $"{from:MMM d} – {to:MMM d, yyyy}",
                _ => $"{from:MMM d, yyyy} – {to:MMM d, yyyy}",
            };

        // UTC bounds of the Philippine days from..to, for querying timestamps: [start, end).
        public static (DateTime Start, DateTime End) UtcBounds(DateOnly from, DateOnly to) =>
            (
                from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset,
                to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) - DemandWeek.PhOffset
            );
    }
}
