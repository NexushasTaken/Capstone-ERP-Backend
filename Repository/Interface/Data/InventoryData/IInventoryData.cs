using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;

namespace ERP.Repository.Interface.Data.InventoryData
{
    public interface IInventoryData : IBaseData
    {
        #region Inventory
        Task<IEnumerable<Inventory>> GetInventoriesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            int statusId,
            InventoryListFilter listFilter,
            CancellationToken cancellation
        );
        IQueryable<Inventory> FilteringQuery(
            IQueryable<Inventory> query,
            string? name,
            int filter,
            int statusId,
            InventoryListFilter listFilter
        );
        Task<int> InventoryCount(string? name, int filter, int statusId, InventoryListFilter listFilter);
        Task<Inventory> GetInventoryWithTracking(int id);
        Task<IEnumerable<Inventory>> GetProductInventoryWithTracking(int productId);
        Task<bool> CheckExistingInventory(int productId);
        Task<IEnumerable<Inventory>> GetInventoriesWithTracking(List<int> Id);
        Task<IEnumerable<InventoryStatusViewModel>> StatusCount();
        #endregion

        #region Adjustments
        Task<IEnumerable<StockAdjustment>> GetAdjustmentsWithoutTracking(int id);
        #endregion

        #region Transaction
        Task<IEnumerable<InventoryTransaction>> GetItemTransactionWithoutTracking(int id);
        #endregion
    }
}
