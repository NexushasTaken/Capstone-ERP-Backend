using ERP.Repository.Model.Orders;

namespace ERP.Repository.Interface.Data.OrderData
{
    public interface IOrderData : IBaseData
    {
        IQueryable<Order> FilteringQuery(IQueryable<Order> query, string name, int filter, int statusId, int orderTypeId);
        Task<IEnumerable<Order>> GetOrdersWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default);
        Task<int> OrdersCount(string? name, int filter, int statusId, int orderTypeId);
        Task<IEnumerable<OrderType>> GetOrderTypesWithoutTracking();
        Task<IEnumerable<OrderStatus>> GetOrderStatusesWithoutTracking();
        Task<Order> GetSingleOrderWithTracking(int id);
        Task<IEnumerable<DeliveryDriver>> GetDeliveryRidersWithoutTracking();
    }
}
