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
    }
}
