using ERP.Repository.ViewModel.Purchases;

namespace ERP.Repository.Interface.Purchases
{
    public interface IPurchaseService
    {
        Task<PurchasePageViewModel> GetPurchases(
            int page,
            int pageSize,
            string? name,
            int statusId,
            int filter,
            CancellationToken cancellation
        );
        Task<IEnumerable<PurchaseStatusCountViewModel>> StatusCount();
        Task InsertPurchase(PurchasePostViewModel purchase);
        Task UpdatePurchase(PurchasePutViewModel purchase);
        Task CancelPurchase(int id);
        Task ReceivePurchase(PurchaseReceiveViewModel receive);
    }
}
