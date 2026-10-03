using ERP.Repository.Model;

namespace ERP.Repository.Model.UserAccounts
{
    public class UserRole : BaseModel
    {
        public string? Role { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = [];
    }
}
