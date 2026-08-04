using ERP.Repository.Configuration;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.ViewModel.UserAccount;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ERP.Repository.Services.TokenManager
{
    public class TokenManagerService(IConfiguration _config) : ITokenManagerService
    {
        public string GenerateJwtToken(UserRoleAndPolicy role)
        {
            var jwtSetting = new JwtSetting();

            _config.GetSection("Jwt").Bind(jwtSetting);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, role.Id.ToString()),
                new (JwtRegisteredClaimNames.Name, role.FirstName!),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new (ClaimTypes.Role, role.Type!),
                new ("position", role.Position!)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSetting.Issuer,
                audience: jwtSetting.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(jwtSetting.ExpireInDays),
                signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public HttpContext SetAccessTokenCookie(string token, HttpContextSetting httpContextSetting, HttpContext context)
        {
            context.Response.Cookies.Append("RefreshToken", token, new CookieOptions
            {
                HttpOnly = httpContextSetting.IsHttpOnly,
                SameSite = httpContextSetting.SameSite == "None" ? SameSiteMode.None : SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(httpContextSetting.ExpireInMinutes)
            });
            return context;
        }


        public string Hashed(string value, string salt)
        {
            var combined = Encoding.UTF8.GetBytes(value + salt);
            var hash = SHA256.HashData(combined);
            var base64 = Convert.ToBase64String(hash);

            return base64;
        }

        public string GenerateSalt()
        {
            var saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public HttpContext FinalizeToken(string token, HttpContext context)
        {
            var httpContextSetting = new HttpContextSetting();
            _config.GetSection("HttpContextSettings").Bind(httpContextSetting);

            SetAccessTokenCookie(token, httpContextSetting, context);

            return context;
        }
    }
}
