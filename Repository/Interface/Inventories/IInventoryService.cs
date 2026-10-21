using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Inventories;
using ERP.Repository.ViewModel.Products;

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
        Task<InventoryPageViewModel> GetInventories(
            int page,
            int pageSize,
            string name,
            int filter,
            int statusId,
            InventoryListFilter listFilter,
            CancellationToken cancellation
        );

        /// <summary>
        /// Insert new item in inventory
        /// </summary>
        /// <param name="inventory">Required field for inventory</param>
        /// <returns></returns>
        Task InsertItem(InventoryPostViewModel inventory);
        Task CreateOpeningStock(Product product, int quantity, int reorderPoint, DateTime now);
        Task DeleteItem(int id);
        Task UpdateInventory(InventoryUpdateViewModel inventory);
        Task Adjust(StockAdjustmentPostViewModel adjustment);
        Task<IEnumerable<StockAdjustmentViewModel>> GetAdjustments(int id);
        Task<IEnumerable<InventoryTransactionViewModle>> GetInventoryItemTransaction(int id);
        Task<IEnumerable<InventoryStatusViewModel>> InventoryStatusCount();
        Task<IEnumerable<ProductViewModel>> GetAllProductForInsert();
    }
}
