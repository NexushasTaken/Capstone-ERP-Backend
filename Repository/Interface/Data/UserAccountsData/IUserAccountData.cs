using ERP.Repository.Interface.Data;
using ERP.Repository.Model.UserAccounts;

namespace ERP.Repository.Interface.Data.UserAccountData
{
    public interface IUserAccountData : IBaseData
    {
        Task<UserAccount> GetUserByEmailWithoutTracking(string email, CancellationToken cancellation = default);
        Task<UserAccount> GetUserByEmailWithTracking(string email, CancellationToken cancellation = default);
        Task<UserAccount> GetUserByIdWithoutTracking(int userId, CancellationToken cancellation = default);
        Task<UserAccount> GetUserByIdWithTracking(int userId, CancellationToken cancellation = default);
        Task<IEnumerable<UserAccount>> GetAllUsersWithoutTracking(CancellationToken cancellation = default);
        Task<IEnumerable<UserAccount>> GetAccountsWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default);
        Task<int> AccountTotalCount(string? name, CancellationToken cancellation = default);
        Task<UserRole> GetRoleByName(string roleName, CancellationToken cancellation = default);
    }
}
