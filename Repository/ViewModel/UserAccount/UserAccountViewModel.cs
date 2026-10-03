using System.Text.Json.Serialization;

namespace ERP.Repository.ViewModel.UserAccount
{
    public class UserAccountViewModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class UserAccountViewModelResponse
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
    }

    public class UserRoleAndPolicy : UserAccountViewModelResponse
    {
        public int Id { get; set; }
    }


    public class UserLoginSuccess
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        [JsonIgnore]
        public string? Token { get; set; }
    }
}
