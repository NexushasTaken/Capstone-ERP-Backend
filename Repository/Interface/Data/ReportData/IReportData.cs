using ERP.Repository.ViewModel.Reports;

namespace ERP.Repository.Interface.Data.ReportData
{
    public interface IReportData : IBaseData
    {
        Task<List<StockReportRow>> StockRows(CancellationToken cancellation = default);
        Task<List<PurchaseReportRow>> ReceivedPurchases(
            DateTime start,
            DateTime end,
            CancellationToken cancellation = default
        );
        Task<List<AdjustmentReportRow>> Adjustments(
            DateTime start,
            DateTime end,
            CancellationToken cancellation = default
        );
    }
}
