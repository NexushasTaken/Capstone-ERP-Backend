using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.UserAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP.Controllers.ViewController
{
    [ApiController]
    [Route("api/View")]
    public class ViewController(ResponseHelper _response, IUserAccountService _userAccountService) : ControllerBase
    {
        [Authorize]
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return StatusCode(401, _response.Status(401, false, "Invalid token", null));
            }

            var currentUser = await _userAccountService.GetCurrentUser(userId);

            return StatusCode(200, _response.Status(200, true, "Authorize", currentUser));
        }

    }
}
