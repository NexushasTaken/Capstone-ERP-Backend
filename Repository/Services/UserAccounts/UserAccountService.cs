using ERP.Repository.Configuration.Validation.UserAccounts;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.ViewModel.UserAccount;

namespace ERP.Repository.Services.UserAccounts
{
    public class UserAccountService(
        IUserAccountData _userAccountData,
        ITokenManagerService _tokenManagerService) : IUserAccountService
    {
        public async Task<UserLoginSuccess> Login(UserAccountViewModel user)
        {
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
                FirstName = existingUser.UserInformation.FirstName,
                Type = existingUser.UserType.Type,
                Position = existingUser.UserPosition.Position,
            };

            var accessToken = _tokenManagerService.GenerateJwtToken(jwt);

            return new UserLoginSuccess
            {
                Id = existingUser.Id,
                FirstName = existingUser.UserInformation.FirstName,
                Type = existingUser.UserType.Type,
                Position = existingUser.UserPosition.Position,
            };
        }
    }
}
