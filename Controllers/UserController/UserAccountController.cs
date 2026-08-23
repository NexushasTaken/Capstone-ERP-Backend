using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.ViewModel.UserAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.UserController
{
    [ApiController]
    [Route("api/User")]
    public class UserAccountController(IUserAccountService _userAccountService, ITokenManagerService _tokenManagerService, ResponseHelper _response) : ControllerBase
    {
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserAccountViewModel user)
        {
            var result = await _userAccountService.Login(user);

            _tokenManagerService.FinalizeToken(result.Token, HttpContext);

            result.Token = null;

            return StatusCode(200, _response.Status(200, true, "Successfully Login", result));
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            if (HttpContext.Request.Cookies["AccessToken"] != null)
            {
                HttpContext.Response.Cookies.Delete("AccessToken");
            }

            return StatusCode(200, _response.Status(200, true, "Successfully Logout"));
        }
    }
}