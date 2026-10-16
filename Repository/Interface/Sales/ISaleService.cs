using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.Interface.Sales
{
    public interface ISaleService
    {
        Task<SalesPageViewModel> GetSales(
            int page,
            int pageSize,
            string? name,
            int filter,
            int orderTypeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task<SaleReturnPageViewModel> GetReturns(
            int page,
            int pageSize,
            string? name,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default
        );
        Task ReturnItems(SaleReturnPostViewModel saleReturn);
        Task<SaleDateRangeViewModel> GetSaleDateRange(CancellationToken cancellationToken = default);
        Task<SaleReportFile> GetSalesReportPdf(
            string? period,
            DateOnly? date,
            CancellationToken cancellationToken = default
        );
    }
}
