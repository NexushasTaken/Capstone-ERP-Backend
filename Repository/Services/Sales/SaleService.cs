using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Interface.Sales;
using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.Services.Sales
{
    public class SaleService(ISalesData _sales) : ISaleService
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
    }
}
