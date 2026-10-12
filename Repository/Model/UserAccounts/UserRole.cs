using ERP.Repository.Model;

namespace ERP.Repository.Model.UserAccounts
{
    public class UserRole : KeyModel
    {
        public string? Role { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = [];
    }
}
