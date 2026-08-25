using ERP.Repository.Model.Inventories;

namespace ERP.Repository.Interface.Data.InventoryData
{
    public interface IInventoryData : IBaseData
    {
        #region Inventory
        Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int wareHousePresent, CancellationToken cancellation);
        IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string name, int filter, int statusId, int wareHousePresent);
        Task<int> InventoryCount(string name, int filter, int statusId, int wareHousePresent);
        Task<Inventory> GetInventoryWithTracking(int id);
        Task<int> GetStatusCountInventory(int statudId);
        Task<int> GetInventoryWithNoWareHouseCountWithoutTracking();
        Task<ICollection<Inventory>> GetInventoryWithWareHouseId(int id);
        #endregion

        #region Warehouse
        Task<int> GetIndividualWarehouseCurrentCapacityWithoutTracking(int id);
        Task<List<Warehouse>> GetWarehousesWithoutTracking();
        Task<int> GetIndividualWarehousesMaxCapacityWithoutTracking(int id);
        Task<Warehouse> GetIndividualWareHouseWithTracking(int id);
        #endregion
    }
}
