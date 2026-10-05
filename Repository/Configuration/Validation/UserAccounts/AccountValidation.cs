using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.UserAccount;
using System.Text.RegularExpressions;

namespace ERP.Repository.Configuration.Validation.UserAccounts
{
    public class AccountValidation
    {
        private static readonly string EmailPattern = @"^[\w.-]+@[\w.-]+\.\w{2,}$";
        private const int MinPasswordLength = 8;
        private const int LockedAccountId = 1;

        private static readonly string[] AllowedRoles = ["owner", "secretary"];

        public static void ValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new BadRequest("Email cannot be null or empty.");
            }

            if (!Regex.IsMatch(email, EmailPattern))
            {
                throw new BadRequest("Email is invalid");
            }
        }

        public static void ValidRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role) || !AllowedRoles.Contains(role.ToLower()))
            {
                throw new BadRequest("Role must be owner or secretary");
            }
        }

        public static void ValidPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            {
                throw new BadRequest($"Password must be at least {MinPasswordLength} characters");
            }
        }

        public static void CreateValidation(CreateAccountViewModel account)
        {
            if (string.IsNullOrWhiteSpace(account.FirstName))
            {
                throw new BadRequest("First name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(account.LastName))
            {
                throw new BadRequest("Last name cannot be null or empty.");
            }

            ValidEmail(account.Email);
            ValidPassword(account.Password);
            ValidRole(account.Role);
        }

        public static void RoleUpdateValidation(int id, UpdateAccountRoleViewModel account)
        {
            if (id == LockedAccountId)
            {
                throw new BadRequest("This account's role cannot be changed.");
            }

            ValidRole(account.Role);
        }

        public static void DeleteValidation(int id, int? currentUserId)
        {
            if (id == LockedAccountId)
            {
                throw new BadRequest("This account cannot be deleted.");
            }

            if (id == currentUserId)
            {
                throw new BadRequest("You cannot delete your own account.");
            }
        }

        public static void ProfileValidation(UpdateProfileViewModel profile)
        {
            if (string.IsNullOrWhiteSpace(profile.FirstName))
            {
                throw new BadRequest("First name cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(profile.LastName))
            {
                throw new BadRequest("Last name cannot be null or empty.");
            }
        }

        public static void CredentialsValidation(UpdateCredentialsViewModel credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.CurrentPassword))
            {
                throw new BadRequest("Current password is required.");
            }

            ValidEmail(credentials.Email);

            if (!string.IsNullOrEmpty(credentials.Password))
            {
                ValidPassword(credentials.Password);
            }
        }
    }
}
