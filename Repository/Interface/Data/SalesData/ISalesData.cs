using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.Interface.Data.SalesData
{
    public interface ISalesData : IBaseData
    {
        Task<IEnumerable<SaleTotalViewModel>> GetSalesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task<int> SalesCount(
            string? name,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task<decimal> SalesTotalAmount(
            string? name,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task<(DateTime? First, DateTime? Last)> SaleDateRange(CancellationToken cancellationToken = default);
        Task<List<SaleReportSale>> GetSalesForReport(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default
        );
        Task<Sale?> GetSaleForReturnWithTracking(int saleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SaleReturnViewModel>> GetReturnsWithoutTracking(
            int page,
            int pageSize,
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task<int> ReturnsCount(
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
    }
}
