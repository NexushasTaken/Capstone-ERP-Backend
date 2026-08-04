namespace ERP.Repository.Model.UserAccounts
{
    public class UserAccount : BaseModel
    {
        public int UserTypeId { get; set; }
        public UserType? UserType { get; set; }
        public int UserPositionId { get; set; }
        public UserPosition? UserPosition { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Salt { get; set; }

        public UserInformation? UserInformation { get; set; }
    }
}
