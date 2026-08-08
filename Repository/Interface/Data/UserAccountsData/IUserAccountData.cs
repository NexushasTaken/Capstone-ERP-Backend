using ERP.Repository.Model.UserAccounts;

namespace ERP.Repository.Interface.Data.UserAccountData
{
    public interface IUserAccountData
    {
        Task<UserAccount> GetUserByEmailWithoutTracking(string email, CancellationToken cancellation = default);
        Task<UserAccount> GetUserByEmailWithTracking(string email, CancellationToken cancellation = default);
    }
}
