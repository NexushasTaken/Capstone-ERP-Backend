using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;

namespace ERP.Repository.Interface.Data.InventoryData
{
    public interface IInventoryData : IBaseData
    {
        #region Inventory
        Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(int page, int pageSize, string? name, int filter, int statusId, int wareHousePresent, CancellationToken cancellation);
        IQueryable<Inventory> FilteringQuery(IQueryable<Inventory> query, string name, int filter, int statusId, int wareHousePresent);
        Task<int> InventoryCount(string name, int filter, int statusId, int wareHousePresent);
        Task<Inventory> GetInventoryWithTracking(int id);
        Task<ICollection<Inventory>> GetInventoryWithWareHouseId(int id);
        Task<IEnumerable<Inventory>> GetProductInventoryWithTracking(int productId);
        Task<bool> CheckExistingInventory(string name, int warehouseId);
        Task<IEnumerable<Inventory>> GetInventoriesWithTracking(List<int> Id);
        #endregion

        #region Warehouse
        Task<int> GetIndividualWarehouseCurrentCapacityWithoutTracking(int id);
        Task<List<Warehouse>> GetWarehousesWithoutTracking();
        Task<int> GetIndividualWarehousesMaxCapacityWithoutTracking(int id);
        Task<Warehouse> GetIndividualWareHouseWithTracking(int id);
        #endregion


        #region Inventory Label
        Task<IEnumerable<InventoryLabel>> GetLabelForInsert();
        #endregion

        #region Inventory Damage
        Task<IEnumerable<DamagedInventory>> GetDamageInventoryWithoutTracking(int id);
        #endregion

        #region Transaction
        Task<IEnumerable<InventoryTransaction>> GetItemTransactionWithoutTracking(int id);
        #endregion

        #region Movement Velocity
        Task<IEnumerable<InventoryMovementVelocityViewModel>> GetMovementVelocityWithoutTracking(int cutOffDate, int page, int pageSize);
        Task<int> GetMovementVelocityCount(int cutoffDate);
        #endregion
    }
}
