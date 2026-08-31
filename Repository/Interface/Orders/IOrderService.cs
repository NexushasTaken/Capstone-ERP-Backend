using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Interface.Orders
{
    public interface IOrderService
    {
        Task<OrderPageViewModel> GetOrders(int page, int pageSize, string? name, int filter, int statusId, int orderTypeId, CancellationToken cancellationToken = default);
        Task InsertOrder(List<OrderPostViewModel> order);
    }
}
