using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Sales;

namespace ERP.Repository.Interface.Data.SalesData
{
    public interface ISalesData : IBaseData
    {
        IQueryable<OrderLine> FilteringQuery(IQueryable<OrderLine> query, string? name, int filter, int orderTypeId);
        Task<IEnumerable<SaleTotalViewModel>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int orderTypeId, CancellationToken cancellationToken = default);
        Task<int> OrdersCount(string? name, int filter, int orderTypeId);
    }
}
