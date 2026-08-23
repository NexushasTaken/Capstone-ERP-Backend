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
        /// <param name="searchString">Search for Name in the inventory</param>
        /// <param name="filter">filtering method, 1 - 7 value</param>
        /// <returns></returns>
        Task<InventoryPageViewModel> GetInventories(int page, int pageSize, string searchString, int filter,int statusId, CancellationToken cancellation);
        /// <summary>
        /// Insert new item in inventory
        /// </summary>
        /// <param name="inventory">Required field for inventory</param>  
        /// <returns></returns>
        Task InsertItem(InventoryPostViewModel inventory, int id);
        Task DeleteItem(int id);
        Task<int> StatusInventoryCount(int id);
        Task UpdateInventory(InventoryUpdateViewModel inventory);
    }
}
