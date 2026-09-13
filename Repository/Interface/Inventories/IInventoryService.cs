using ERP.Repository.ViewModel.Inventories;

namespace ERP.Repository.Interface.Inventories
{
    public interface IInventoryService
    {
        /// <summary>
        /// This method will return all inventory record
        /// </summary>
        /// <param name="page">Page you will look into</param>
        /// <param name="pageSize">Size of the result</param>
        /// <param name="name">Search for Name in the inventory</param>
        /// <param name="filter">filtering method, 1 - 7 value</param>
        /// <returns></returns>
        Task<InventoryPageViewModel> GetInventories(int page, int pageSize, string name, int filter,int statusId, int wareHousePresent, CancellationToken cancellation);
        /// <summary>
        /// Insert new item in inventory
        /// </summary>
        /// <param name="inventory">Required field for inventory</param>  
        /// <returns></returns>
        Task InsertItem(InventoryPostViewModel inventory);
        Task DeleteItem(int id);
        Task UpdateInventory(InventoryUpdateViewModel inventory);
        Task MarkAsDamaged(InventoryDamagePostViewModel damaged);
        Task InventoryTransaction(InventoryTransactionPostViewModel transaction);
        Task<IEnumerable<InventoryWareHouseViewModel>> GetWarehouses();
        Task NewWareHouse(InventoryWareHousePostViewModel wareHouse);
        Task UpdateWareHouse(InventoryWareHouseUpdateViewModel wareHouse);
        Task DeleteWareHouse(int id);
        Task<IEnumerable<InventoryLabelViewModel>> GetInventoryLabels();
        Task<IEnumerable<InventoryDamageViewModel>> GetDamageInventory(int id);
        Task<IEnumerable<InventoryTransactionViewModle>> GetInventoryItemTransaction(int id);
        Task<InventoryMovementVelocityPageViewModel> GetMovementVelocity(int cutOffDate, int page, int pageSize);
        Task Restock(InventoryRestockViewModel inventory);
        Task<IEnumerable<InventoryStatusViewModel>> InventoryStatusCount();
    }
}
