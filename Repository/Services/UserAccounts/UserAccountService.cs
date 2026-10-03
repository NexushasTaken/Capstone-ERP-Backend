using ERP.Repository.Configuration.Validation.UserAccounts;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.Model.UserAccounts;
using ERP.Repository.ViewModel.UserAccount;

namespace ERP.Repository.Services.UserAccounts
{
    public class UserAccountService(
        IUserAccountData _userAccountData,
        ITokenManagerService _tokenManagerService) : IUserAccountService
    {
        public async Task<UserLoginSuccess> Login(UserAccountViewModel user)
        {
            //var salt = _tokenManagerService.GenerateSalt();

            //var pass = _tokenManagerService.Hashed(user.Password, salt);


            // Implement your login logic here

            LoginValidation.NotNullEmailAndPassword(user);

            var existingUser = await _userAccountData.GetUserByEmailWithoutTracking(user.Email);

            if(existingUser == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            string password = _tokenManagerService.Hashed(user.Password, existingUser.Salt);

            if(password != existingUser.Password)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var jwt = new UserRoleAndPolicy
            {
                Id = existingUser.Id,
                FirstName = existingUser.UserInformation?.FirstName,
                LastName = existingUser.UserInformation?.LastName,
                Role = existingUser.UserRole?.Role,
            };

            var accessToken = _tokenManagerService.GenerateJwtToken(jwt);

            return ToCurrentUser(existingUser, accessToken);
        }

        public async Task<UserLoginSuccess> GetCurrentUser(int userId)
        {
            var user = await _userAccountData.GetUserByIdWithoutTracking(userId);

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            return ToCurrentUser(user, null);
        }

        public async Task<IEnumerable<UserListViewModel>> GetUsers(CancellationToken cancellation = default)
        {
            var users = await _userAccountData.GetAllUsersWithoutTracking(cancellation);

            return users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                FullName = $"{u.UserInformation?.FirstName} {u.UserInformation?.LastName}".Trim(),
                IsActive = u.IsActive == true
            });
        }

        private UserLoginSuccess ToCurrentUser(UserAccount user, string? token)
        {
            return new UserLoginSuccess
            {
                Id = user.Id,
                FirstName = user.UserInformation?.FirstName,
                LastName = user.UserInformation?.LastName,
                Role = user.UserRole?.Role,
                Token = token
            };
        }
    }
}
