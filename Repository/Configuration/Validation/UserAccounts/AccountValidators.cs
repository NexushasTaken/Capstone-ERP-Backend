using ERP.Repository.ViewModel.UserAccount;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.UserAccounts
{
    public static class AccountRules
    {
        public const string EmailPattern = @"^[\w.-]+@[\w.-]+\.\w{2,}$";
        public const int MinPasswordLength = 8;
        public static readonly string[] AllowedRoles = ["owner", "secretary"];

        public static IRuleBuilderOptions<T, string?> ValidEmail<T>(this IRuleBuilder<T, string?> rule) =>
            rule.NotEmpty().WithMessage("Email cannot be null or empty.")
                .Matches(EmailPattern).WithMessage("Email is invalid");

        public static IRuleBuilderOptions<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> rule) =>
            rule.Must(p => !string.IsNullOrWhiteSpace(p) && p.Length >= MinPasswordLength)
                .WithMessage($"Password must be at least {MinPasswordLength} characters");

        public static IRuleBuilderOptions<T, string?> ValidRole<T>(this IRuleBuilder<T, string?> rule) =>
            rule.Must(r => !string.IsNullOrWhiteSpace(r) && AllowedRoles.Contains(r.ToLower()))
                .WithMessage("Role must be owner or secretary");
    }

    public class LoginValidator : AbstractValidator<UserAccountViewModel>
    {
        public LoginValidator()
        {
            RuleFor(u => u.Email).ValidEmail();
            RuleFor(u => u.Password).NotEmpty().WithMessage("Password cannot be null or empty.");
        }
    }

    public class CreateAccountValidator : AbstractValidator<CreateAccountViewModel>
    {
        public CreateAccountValidator()
        {
            RuleFor(a => a.FirstName).NotEmpty().WithMessage("First name cannot be null or empty.");
            RuleFor(a => a.LastName).NotEmpty().WithMessage("Last name cannot be null or empty.");
            RuleFor(a => a.Email).ValidEmail();
            RuleFor(a => a.Password).ValidPassword();
            RuleFor(a => a.Role).ValidRole();
        }
    }

    public class UpdateRoleValidator : AbstractValidator<UpdateAccountRoleViewModel>
    {
        public UpdateRoleValidator()
        {
            RuleFor(a => a.Role).ValidRole();
        }
    }

    public class UpdateProfileValidator : AbstractValidator<UpdateProfileViewModel>
    {
        public UpdateProfileValidator()
        {
            RuleFor(p => p.FirstName).NotEmpty().WithMessage("First name cannot be null or empty.");
            RuleFor(p => p.LastName).NotEmpty().WithMessage("Last name cannot be null or empty.");
        }
    }

    public class UpdateCredentialsValidator : AbstractValidator<UpdateCredentialsViewModel>
    {
        public UpdateCredentialsValidator()
        {
            RuleFor(c => c.CurrentPassword).NotEmpty().WithMessage("Current password is required.");
            RuleFor(c => c.Email).ValidEmail();
            RuleFor(c => c.Password).ValidPassword().When(c => !string.IsNullOrEmpty(c.Password));
        }
    }
}
