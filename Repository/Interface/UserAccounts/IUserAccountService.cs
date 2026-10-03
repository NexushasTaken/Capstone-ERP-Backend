using ERP.Repository.ViewModel.UserAccount;

namespace ERP.Repository.Interface.UserAccounts
{
    public interface IUserAccountService
    {
        Task<UserLoginSuccess> Login(UserAccountViewModel user);
        Task<UserLoginSuccess> GetCurrentUser(int userId);
    }
}
