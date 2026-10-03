using ERP.Repository.Model;

namespace ERP.Repository.Model.UserAccounts
{
    public class UserAccount : BaseModel
    {
        public int UserRoleId { get; set; }
        public UserRole? UserRole { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Salt { get; set; }

        public UserInformation? UserInformation { get; set; }
    }
}
