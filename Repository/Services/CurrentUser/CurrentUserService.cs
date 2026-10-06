using System.Security.Claims;
using ERP.Repository.Interface.CurrentUser;

namespace ERP.Repository.Services.CurrentUser
{
    public class CurrentUserService(IHttpContextAccessor _httpContextAccessor) : ICurrentUserService
    {
        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }
    }
}
