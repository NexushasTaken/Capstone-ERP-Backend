using ERP.Repository.Model.Orders;

namespace ERP.Repository.Interface.Data.OrderData
{
    public interface IDriverData : IBaseData
    {
        IQueryable<DeliveryDriver> FilterQuery(IQueryable<DeliveryDriver> query, string? name, int filter);
        Task<IEnumerable<DeliveryDriver>> GetDriversWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        );
        Task<int> DriverCount(string? name, int filter);
        Task<DeliveryDriver> GetSingleDriverWithTracking(int id);
    }
}
