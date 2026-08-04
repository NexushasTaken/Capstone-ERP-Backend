namespace ERP.Repository.Model.UserAccounts
{
    public class UserInformation : BaseModel
    {
        public int UserAccountId { get; set; }
        public UserAccount? UserAccount { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
