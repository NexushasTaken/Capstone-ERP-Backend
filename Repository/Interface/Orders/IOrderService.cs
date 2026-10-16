using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Interface.Orders
{
    public interface IOrderService
    {
        Task<OrderPageViewModel> GetOrders(
            int page,
            int pageSize,
            string? name,
            int filter,
            int statusId,
            int orderTypeId,
            CancellationToken cancellationToken = default
        );
        Task<List<string>?> InsertOrder(OrderPostViewModel order);
        Task UpdateOrder(OrderPutViewModel order);
        Task<IEnumerable<OrderTypeViewModel>> GetOrderTypes();
        Task<IEnumerable<OrderStatusViewModel>> GetOrderStatuses();
        Task UpdateOrderStatus(OrderStatusPostViewModel status);
        Task<IEnumerable<OrderDeliveryDriverViewModel>> GetDeliveryDrivers();
        Task<DeliveryDriverPageViewModel> GetDrivers(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        );
        Task AddDriver(DeliveryDriverPostViewModel driver);
        Task<int> ImportDrivers(DriverImportViewModel import);
        Task UpdateDriver(DeliveryDriverPatchViewModel driver);
        Task DeleteDriver(int id);
        Task<IEnumerable<OrderStatusCount>> OrdersStatusCount();
    }
}
