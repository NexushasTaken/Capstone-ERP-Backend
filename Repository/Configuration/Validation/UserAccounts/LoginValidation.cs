using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.UserAccount;
using System.Text.RegularExpressions;

namespace ERP.Repository.Configuration.Validation.UserAccounts
{
    public class LoginValidation
    {

        private static readonly string EmailPattern = @"^[\w.-]+@[\w.-]+\.\w{2,}$";

        public static void NotNullEmailAndPassword(UserAccountViewModel user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new BadRequest("Email cannot be null or empty.");
            }
            if (!Regex.IsMatch(user.Email, EmailPattern))
            {
                throw new BadRequest("Email is invalid");
            }
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new BadRequest("Password cannot be null or empty.");
            }
        }
    }
}
