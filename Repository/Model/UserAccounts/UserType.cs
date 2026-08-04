namespace ERP.Repository.Model.UserAccounts
{
    public class UserType : BaseModel
    {
        public string? Type { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = [];
    }
}
