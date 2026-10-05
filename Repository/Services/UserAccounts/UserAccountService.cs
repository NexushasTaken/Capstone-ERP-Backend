using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Configuration.Validation.UserAccounts;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.Model.UserAccounts;
using ERP.Repository.ViewModel.UserAccount;
using FluentValidation;

namespace ERP.Repository.Services.UserAccounts
{
    public class UserAccountService(
        IUserAccountData _userAccountData,
        ITokenManagerService _tokenManagerService,
        IAuditLogService _auditLog,
        IValidator<UserAccountViewModel> _loginValidator,
        IValidator<CreateAccountViewModel> _createValidator,
        IValidator<UpdateAccountRoleViewModel> _roleValidator,
        IValidator<UpdateProfileViewModel> _profileValidator,
        IValidator<UpdateCredentialsViewModel> _credentialsValidator) : IUserAccountService
    {
        private const int LockedAccountId = 1;

        public async Task<UserLoginSuccess> Login(UserAccountViewModel user)
        {
            //var salt = _tokenManagerService.GenerateSalt();

            //var pass = _tokenManagerService.Hashed(user.Password, salt);


            // Implement your login logic here

            await _loginValidator.EnsureValidAsync(user);

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

        public async Task<AccountPageViewModel> GetAccounts(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default)
        {
            PageQueryValidator.Ensure(page, pageSize);

            var accounts = await _userAccountData.GetAccountsWithoutTracking(page, pageSize, name, filter, cancellation);
            var count = await _userAccountData.AccountTotalCount(name, cancellation);

            return new AccountPageViewModel
            {
                Accounts = accounts.Select(a => new AccountListItemViewModel
                {
                    Id = a.Id,
                    Role = a.UserRole?.Role,
                    FirstName = a.UserInformation?.FirstName,
                    LastName = a.UserInformation?.LastName,
                    Email = a.Email
                }),
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count
            };
        }

        public async Task CreateAccount(CreateAccountViewModel account)
        {
            await _createValidator.EnsureValidAsync(account);

            var existingUser = await _userAccountData.GetUserByEmailWithoutTracking(account.Email!);

            if (existingUser != null)
            {
                throw new ValidationFailed("email", "An account with this email already exists.");
            }

            var role = await _userAccountData.GetRoleByName(account.Role!);

            if (role == null)
            {
                throw new BadRequest("Role must be owner or secretary");
            }

            var now = DateTime.UtcNow;
            var salt = _tokenManagerService.GenerateSalt();
            var hashedPassword = _tokenManagerService.Hashed(account.Password!, salt);

            var newAccount = new UserAccount
            {
                Email = account.Email,
                Password = hashedPassword,
                Salt = salt,
                UserRoleId = role.Id,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
                UserInformation = new UserInformation
                {
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Created_By = _auditLog.CurrentUserId,
                    Created_At = now,
                    IsActive = true
                }
            };

            await _userAccountData.Save(newAccount);

            _auditLog.Log(AuditModuleEnum.Account, AuditActionEnum.Create, $"Created account for '{account.Email}'", newAccount.Id, now);
            await _userAccountData.SaveChanges();
        }

        public async Task UpdateAccountRole(int id, UpdateAccountRoleViewModel account)
        {
            if (id == LockedAccountId)
            {
                throw new BadRequest("This account's role cannot be changed.");
            }

            await _roleValidator.EnsureValidAsync(account);

            var existingAccount = await _userAccountData.GetUserByIdWithTracking(id);

            if (existingAccount == null)
            {
                throw new NotFound($"Account with ID {id} not found.");
            }

            var role = await _userAccountData.GetRoleByName(account.Role!);

            if (role == null)
            {
                throw new BadRequest("Role must be owner or secretary");
            }

            var now = DateTime.UtcNow;

            existingAccount.UserRoleId = role.Id;
            existingAccount.Updated_By = _auditLog.CurrentUserId;
            existingAccount.Updated_At = now;

            _auditLog.Log(AuditModuleEnum.Account, AuditActionEnum.Update, $"Updated role for account {id} to '{role.Role}'", id, now);
            await _userAccountData.SaveChanges();
        }

        // Soft delete: the account is deactivated, not removed.
        public async Task DeleteAccount(int id)
        {
            if (id == LockedAccountId)
            {
                throw new BadRequest("This account cannot be deleted.");
            }

            if (id == _auditLog.CurrentUserId)
            {
                throw new BadRequest("You cannot delete your own account.");
            }

            var existingAccount = await _userAccountData.GetUserByIdWithTracking(id);

            if (existingAccount == null)
            {
                throw new NotFound($"Account with ID {id} not found.");
            }

            var now = DateTime.UtcNow;

            existingAccount.IsActive = false;
            existingAccount.Deleted_By = _auditLog.CurrentUserId;
            existingAccount.Deleted_At = now;

            if (existingAccount.UserInformation != null)
            {
                existingAccount.UserInformation.IsActive = false;
                existingAccount.UserInformation.Deleted_By = _auditLog.CurrentUserId;
                existingAccount.UserInformation.Deleted_At = now;
            }

            _auditLog.Log(AuditModuleEnum.Account, AuditActionEnum.Delete, $"Deleted account '{existingAccount.Email}'", id, now);
            await _userAccountData.SaveChanges();
        }

        public async Task UpdateProfile(int userId, UpdateProfileViewModel profile)
        {
            await _profileValidator.EnsureValidAsync(profile);

            var existingAccount = await _userAccountData.GetUserByIdWithTracking(userId);

            if (existingAccount == null)
            {
                throw new NotFound("Account not found.");
            }

            var now = DateTime.UtcNow;

            existingAccount.UserInformation!.FirstName = profile.FirstName;
            existingAccount.UserInformation!.LastName = profile.LastName;
            existingAccount.UserInformation!.Updated_By = userId;
            existingAccount.UserInformation!.Updated_At = now;

            await _userAccountData.SaveChanges();
        }

        public async Task<CredentialsViewModel> GetCredentials(int userId)
        {
            var existingAccount = await _userAccountData.GetUserByIdWithoutTracking(userId);

            if (existingAccount == null)
            {
                throw new NotFound("Account not found.");
            }

            return new CredentialsViewModel { Email = existingAccount.Email };
        }

        public async Task UpdateCredentials(int userId, UpdateCredentialsViewModel credentials)
        {
            await _credentialsValidator.EnsureValidAsync(credentials);

            var existingAccount = await _userAccountData.GetUserByIdWithTracking(userId);

            if (existingAccount == null)
            {
                throw new NotFound("Account not found.");
            }

            var hashedCurrentPassword = _tokenManagerService.Hashed(credentials.CurrentPassword!, existingAccount.Salt);

            if (hashedCurrentPassword != existingAccount.Password)
            {
                throw new UnauthorizedAccessException("Current password is incorrect.");
            }

            if (!string.Equals(credentials.Email, existingAccount.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailOwner = await _userAccountData.GetUserByEmailWithoutTracking(credentials.Email!);

                if (emailOwner != null && emailOwner.Id != userId)
                {
                    throw new ValidationFailed("email", "An account with this email already exists.");
                }
            }

            existingAccount.Email = credentials.Email;
            existingAccount.Updated_By = userId;
            existingAccount.Updated_At = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(credentials.Password))
            {
                var newSalt = _tokenManagerService.GenerateSalt();

                existingAccount.Salt = newSalt;
                existingAccount.Password = _tokenManagerService.Hashed(credentials.Password, newSalt);
            }

            await _userAccountData.SaveChanges();
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
