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
        public async Task<SalesPageViewModel> GetSales(int page, int pageSize, string? name, int filter, int orderTypeId, CancellationToken cancellationToken = default)
        {
            GlobalValidation.PageValidation(page, pageSize);

            var result = await _sales.GetOrdersWithoutTracking(page, pageSize, name, filter, orderTypeId, cancellationToken);
            var totalCount = await _sales.OrdersCount(name, filter, orderTypeId);


            var sales = result
                .GroupBy(o => o.OrderId)
                .Select(o => new SaleTotalViewModel
                {
                    Id = o.First().OrderId,
                    OrderType = o.First().Order.OrderType.Type,
                    OrderStatus = o.First().Order.OrderStatus.Status,
                    DriverName = o.First().Order.DeliveryDriver == null ? "" : string.Concat(o.First().Order.DeliveryDriver.FirstName, " ", o.First().Order.DeliveryDriver.LastName),
                    CustomerName = o.First().Order.CustomerName,
                    PickUpAddress = o.First().Order.PibkupAddress,
                    DeliveryAddress = o.First().Order.DeliveryAddress,
                    Orders = o.GroupBy(g => g.ProductId)
                    .Select(g => new OrderViewModel
                    {
                        ProductName = g.First().Product.Name,
                        Quantity = g.Sum(x => x.Quantity),
                        Price = g.First().Product.Price,
                        TotalAmount = g.Sum(x => x.Amount)
                    }).ToList(),
                    Total = o.Sum(g => g.Amount),
                    Created_At = o.First().Created_At
                }).ToList();


            return new SalesPageViewModel
            {
                Sales = sales,
                PageCount = (int)Math.Ceiling((double)totalCount / pageSize),
                Rows = totalCount
            };
        }
    }
}
