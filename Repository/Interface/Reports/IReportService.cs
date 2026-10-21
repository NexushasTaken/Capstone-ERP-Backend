using ERP.Repository.ViewModel.Reports;

namespace ERP.Repository.Interface.Reports
{
    public interface IReportService
    {
        Task<ReportFile> StockReportPdf(CancellationToken cancellation = default);
        Task<ReportFile> PurchaseReportPdf(string? period, DateOnly? date, CancellationToken cancellation = default);
        Task<ReportFile> AdjustmentReportPdf(string? period, DateOnly? date, CancellationToken cancellation = default);
    }
}
