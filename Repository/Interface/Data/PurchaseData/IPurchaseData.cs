using ERP.Repository.Model.Inventories;
using ERP.Repository.Model.Purchases;
using ERP.Repository.ViewModel.Purchases;

namespace ERP.Repository.Interface.Data.PurchaseData
{
    public interface IPurchaseData : IBaseData
    {
        Task<IEnumerable<PurchaseViewModel>> GetPurchasesWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int statusId,
            int filter,
            CancellationToken cancellation = default
        );
        Task<int> PurchaseCount(string? name, int statusId, CancellationToken cancellation = default);
        Task<IEnumerable<PurchaseStatusCountViewModel>> StatusCount();
        Task<Purchase?> GetPurchaseWithTracking(int id);
        Task<Dictionary<int, string>> GetActiveProductNames(IEnumerable<int> productIds);
        Task<Dictionary<int, Inventory>> GetActiveInventoriesByProductWithTracking(IEnumerable<int> productIds);
    }
}
