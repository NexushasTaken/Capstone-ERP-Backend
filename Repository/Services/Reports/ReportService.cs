using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.CurrentUser;
using ERP.Repository.Interface.Data.ReportData;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.Reports;
using ERP.Repository.ViewModel.Reports;
using QuestPDF.Fluent;

namespace ERP.Repository.Services.Reports
{
    // The PDF reports on the Reports page, besides Sales (SaleService). Each one gathers its rows here,
    // and its *ReportDocument only draws them.
    public class ReportService(IReportData _reports, IUserAccountData _users, ICurrentUserService _currentUser)
        : IReportService
    {
        public async Task<ReportFile> StockReportPdf(CancellationToken cancellation = default)
        {
            var report = new StockReportViewModel { Rows = await _reports.StockRows(cancellation) };
            await Stamp(report, cancellation);

            return new ReportFile(
                new StockReportDocument(report).GeneratePdf(),
                $"stock-on-hand-{report.GeneratedAt:yyyy-MM-dd}.pdf"
            );
        }

        public async Task<ReportFile> PurchaseReportPdf(
            string? period,
            DateOnly? date,
            CancellationToken cancellation = default
        )
        {
            var parsed = ReportPeriods.Parse(period);
            var (from, to) = ReportPeriods.Range(parsed, date ?? DemandWeek.PhToday());
            var (start, end) = ReportPeriods.UtcBounds(from, to);

            var report = new PurchaseReportViewModel
            {
                Period = parsed,
                From = from,
                To = to,
                Purchases = await _reports.ReceivedPurchases(start, end, cancellation),
            };
            await Stamp(report, cancellation);

            return new ReportFile(
                new PurchaseReportDocument(report).GeneratePdf(),
                $"purchases-{parsed.ToString().ToLowerInvariant()}-{from:yyyy-MM-dd}.pdf"
            );
        }

        public async Task<ReportFile> AdjustmentReportPdf(
            string? period,
            DateOnly? date,
            CancellationToken cancellation = default
        )
        {
            var parsed = ReportPeriods.Parse(period);
            var (from, to) = ReportPeriods.Range(parsed, date ?? DemandWeek.PhToday());
            var (start, end) = ReportPeriods.UtcBounds(from, to);

            var report = new AdjustmentReportViewModel
            {
                Period = parsed,
                From = from,
                To = to,
                Rows = await _reports.Adjustments(start, end, cancellation),
            };
            await Stamp(report, cancellation);

            return new ReportFile(
                new AdjustmentReportDocument(report).GeneratePdf(),
                $"adjustments-{parsed.ToString().ToLowerInvariant()}-{from:yyyy-MM-dd}.pdf"
            );
        }

        // When (Philippine time) and by whom, for the header.
        private async Task Stamp(ReportHeader report, CancellationToken cancellation)
        {
            report.GeneratedAt = DateTime.UtcNow + DemandWeek.PhOffset;

            if (_currentUser.UserId is int userId)
            {
                var user = await _users.GetUserByIdWithoutTracking(userId, cancellation);
                report.GeneratedBy = $"{user?.UserInformation?.FirstName} {user?.UserInformation?.LastName}".Trim();
            }
        }
    }
}
