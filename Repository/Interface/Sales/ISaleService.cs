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
    }
}
