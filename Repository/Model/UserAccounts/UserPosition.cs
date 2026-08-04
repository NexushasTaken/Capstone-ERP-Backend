namespace ERP.Repository.Model.UserAccounts
{
    public class UserPosition : BaseModel
    {
        public string? Position { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = [];
    }
}
