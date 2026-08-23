using ERP.Repository.Model.Inventories;

namespace ERP.Repository.Interface.Data.InventoryData
{
    public interface IInventoryData : IBaseData
    {
        Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(int page, int pageSize, string? searchString, int filter, int statusId, CancellationToken cancellation);
        IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string searchString, int filter, int statusId);
        Task<int> InventoryCount(string searchString, int filter, int statusId);
        Task<Inventory> GetInventoryWithTracking(int id);
        Task<int> GetStatusCountInventory(int statudId);
    }
}
