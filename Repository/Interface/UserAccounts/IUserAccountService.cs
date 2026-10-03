using ERP.Repository.ViewModel.UserAccount;

namespace ERP.Repository.Interface.UserAccounts
{
    public interface IUserAccountService
    {
        Task<UserLoginSuccess> Login(UserAccountViewModel user);
        Task<UserLoginSuccess> GetCurrentUser(int userId);
        Task<IEnumerable<UserListViewModel>> GetUsers(CancellationToken cancellation = default);
        Task<IEnumerable<AccountListItemViewModel>> GetAccounts(CancellationToken cancellation = default);
        Task CreateAccount(CreateAccountViewModel account);
        Task UpdateAccountRole(int id, UpdateAccountRoleViewModel account);
        Task UpdateProfile(int userId, UpdateProfileViewModel profile);
        Task<CredentialsViewModel> GetCredentials(int userId);
        Task UpdateCredentials(int userId, UpdateCredentialsViewModel credentials);
    }
}
