using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Interface.Data.OrderData
{
    public interface IOrderData : IBaseData
    {

        #region Orders
        IQueryable<OrderLine> FilteringQuery(IQueryable<OrderLine> query, string? name, int filter, int statusId, int orderTypeId);
        Task<IEnumerable<OrderTotalViewModel>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default);
        Task<int> OrdersCount(string? name, int filter, int statusId, int orderTypeId);
        Task<IEnumerable<OrderType>> GetOrderTypesWithoutTracking();
        Task<IEnumerable<OrderStatus>> GetOrderStatusesWithoutTracking();
        Task<Order> GetSingleOrderWithTracking(int id);
        #endregion

        #region Drivers
        Task<IEnumerable<DeliveryDriver>> GetDeliveryRidersWithoutTracking();
        Task<DeliveryDriver> ValidateDriverWithoutTracking(int? driverId);
        Task<IEnumerable<OrderLine>> GetOrderLinesWithTracking(int id);
        #endregion
    }
}
