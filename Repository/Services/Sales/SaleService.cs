using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.CurrentUser;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.Sales;
using ERP.Repository.ViewModel.Sales;
using QuestPDF.Fluent;

namespace ERP.Repository.Services.Sales
{
    public class SaleService(ISalesData _sales, IUserAccountData _users, ICurrentUserService _currentUser)
        : ISaleService
    {
        public async Task<SalesPageViewModel> GetSales(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            if (from > to)
            {
                throw new BadRequest("The start date must be on or before the end date");
            }

            var result = await _sales.GetSalesWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                orderTypeId,
                from,
                to,
                cancellationToken
            );
            var totalCount = await _sales.SalesCount(name, orderTypeId, from, to, cancellationToken);
            var totalAmount = await _sales.SalesTotalAmount(name, orderTypeId, from, to, cancellationToken);

            return new SalesPageViewModel
            {
                Sales = result,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount,
                TotalAmount = totalAmount,
            };
        }

        public async Task<SaleDateRangeViewModel> GetSaleDateRange(CancellationToken cancellationToken = default)
        {
            var (first, last) = await _sales.SaleDateRange(cancellationToken);

            return new SaleDateRangeViewModel { First = PhDay(first), Last = PhDay(last) };
        }

        public async Task<SaleReportFile> GetSalesReportPdf(
            string? period,
            DateOnly? date,
            CancellationToken cancellationToken = default
        )
        {
            var report = await GetSalesReport(ParsePeriod(period), date ?? DemandWeek.PhToday(), cancellationToken);
            var content = new SalesReportDocument(report).GeneratePdf();
            var fileName = $"sales-{report.Period.ToString().ToLowerInvariant()}-{report.From:yyyy-MM-dd}.pdf";

            return new SaleReportFile(content, fileName);
        }

        private async Task<SaleReportViewModel> GetSalesReport(
            SaleReportPeriod period,
            DateOnly date,
            CancellationToken cancellationToken
        )
        {
            var (from, to) = period switch
            {
                SaleReportPeriod.Weekly => (DemandWeek.StartOf(date), DemandWeek.StartOf(date).AddDays(6)),
                SaleReportPeriod.Monthly => (
                    new DateOnly(date.Year, date.Month, 1),
                    new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1)
                ),
                _ => (date, date),
            };

            var sales = await _sales.GetSalesForReport(from, to, cancellationToken);

            var gross = sales.Sum(s => s.Subtotal);
            var net = sales.Sum(s => s.Total);

            return new SaleReportViewModel
            {
                Period = period,
                From = from,
                To = to,
                GeneratedAt = DateTime.UtcNow + DemandWeek.PhOffset,
                GeneratedBy = await CurrentUserName(cancellationToken),
                SalesCount = sales.Count,
                Gross = gross,
                // The stored total is what was charged, so the discount is whatever makes up the difference.
                Discount = gross - net,
                Net = net,
                Average = sales.Count == 0 ? 0 : net / sales.Count,
                Sales = period == SaleReportPeriod.Daily ? sales : [],
                Rows = period switch
                {
                    SaleReportPeriod.Weekly => DayRows(sales, from, to),
                    SaleReportPeriod.Monthly => WeekRows(sales, from, to),
                    _ => [],
                },
                TopProducts = TopProducts(sales),
            };
        }

        private static SaleReportPeriod ParsePeriod(string? period) =>
            period?.Trim().ToLowerInvariant() switch
            {
                "daily" => SaleReportPeriod.Daily,
                "weekly" => SaleReportPeriod.Weekly,
                "monthly" => SaleReportPeriod.Monthly,
                _ => throw new BadRequest("The period must be daily, weekly or monthly"),
            };

        // One row per day, days without sales included.
        private static List<SaleReportRow> DayRows(List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var rows = new List<SaleReportRow>();

            for (var day = from; day <= to; day = day.AddDays(1))
            {
                rows.Add(Row($"{day:ddd, MMM d}", sales, day, day));
            }

            return rows;
        }

        // One row per Monday-to-Sunday week, cut at the month's first and last day.
        private static List<SaleReportRow> WeekRows(List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var rows = new List<SaleReportRow>();
            var number = 1;

            for (var start = from; start <= to; number++)
            {
                var end = DemandWeek.StartOf(start).AddDays(6);
                if (end > to)
                {
                    end = to;
                }

                var days = start == end ? $"{start:MMM d}" : $"{start:MMM d}–{end:MMM d}";
                rows.Add(Row($"Week {number}: {days}", sales, start, end));
                start = end.AddDays(1);
            }

            return rows;
        }

        private static SaleReportRow Row(string label, List<SaleReportSale> sales, DateOnly from, DateOnly to)
        {
            var inRange = sales.Where(s => PhDay(s.Created_At) is { } day && day >= from && day <= to).ToList();
            var gross = inRange.Sum(s => s.Subtotal);
            var net = inRange.Sum(s => s.Total);

            return new SaleReportRow
            {
                Label = label,
                Sales = inRange.Count,
                Gross = gross,
                Discount = gross - net,
                Net = net,
            };
        }

        private static List<SaleReportProduct> TopProducts(List<SaleReportSale> sales) =>
            sales
                .SelectMany(s => s.Lines)
                .GroupBy(l => l.ProductId)
                .Select(g => new SaleReportProduct
                {
                    Name = g.First().ProductName,
                    Quantity = g.Sum(l => l.Quantity),
                    Revenue = g.Sum(l => l.Amount),
                })
                .OrderByDescending(p => p.Revenue)
                .ThenByDescending(p => p.Quantity)
                .ThenBy(p => p.Name)
                .Take(10)
                .ToList();

        private async Task<string> CurrentUserName(CancellationToken cancellationToken)
        {
            if (_currentUser.UserId is not { } userId)
            {
                return "";
            }

            var user = await _users.GetUserByIdWithoutTracking(userId, cancellationToken);

            return $"{user?.UserInformation?.FirstName} {user?.UserInformation?.LastName}".Trim();
        }

        // The Philippine day a UTC time falls on, the same day rule the list's from/to filter uses.
        private static DateOnly? PhDay(DateTime? utc) =>
            utc is { } value ? DateOnly.FromDateTime(value + DemandWeek.PhOffset) : null;
    }
}
