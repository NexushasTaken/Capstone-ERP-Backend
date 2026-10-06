using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.SalesData;
using ERP.Repository.Interface.Sales;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;
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
            CancellationToken cancellationToken = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var result = await _sales.GetOrdersWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                orderTypeId,
                cancellationToken
            );
            var totalCount = await _sales.OrdersCount(name, filter, orderTypeId);

            return new SalesPageViewModel
            {
                Sales = result,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount,
            };
        }
    }
}
