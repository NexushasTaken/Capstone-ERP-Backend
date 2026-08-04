using ERP.Repository.Configuration;
using ERP.Repository.ViewModel.UserAccount;

namespace ERP.Repository.Interface.TokenManager
{
    public interface ITokenManagerService
    {
        string GenerateJwtToken(UserRoleAndPolicy role);
        HttpContext SetAccessTokenCookie(string token, HttpContextSetting httpContextSetting, HttpContext context);
        string Hashed(string value, string salt);
        string GenerateSalt();
        HttpContext FinalizeToken(string token, HttpContext context);
    }
}
