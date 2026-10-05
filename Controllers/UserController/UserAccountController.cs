using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.TokenManager;
using ERP.Repository.Interface.UserAccounts;
using ERP.Repository.ViewModel.UserAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ERP.Controllers.UserController
{
    [ApiController]
    [Route("api/User")]
    public class UserAccountController(IUserAccountService _userAccountService, ITokenManagerService _tokenManagerService, ResponseHelper _response) : ControllerBase
    {
        private int CurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid token");
            }

            return userId;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserAccountViewModel user)
        {
            var result = await _userAccountService.Login(user);

            _tokenManagerService.FinalizeToken(result.Token, HttpContext);

            return StatusCode(200, _response.Status(200, true, "Successfully Login", result));
        }

        /// <summary>
        /// Retrieves all users including deactivated ones, used for the Audit Logs user filter.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetUsers(CancellationToken cancellation = default)
        {
            var users = await _userAccountService.GetUsers(cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", users));
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

        /// <summary>
        /// Retrieves one page of accounts (owner only).
        /// </summary>
        /// <param name="name">case-insensitive search on first name, last name, email and role; a number or ACC-n also matches the Id</param>
        /// <param name="filter">sort: 0 Id, 1 role A - Z, 2 first name A - Z, 3 first name Z - A</param>
        [Authorize(Roles = "owner")]
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? name = "", [FromQuery] int filter = 0, CancellationToken cancellation = default)
        {
            var accounts = await _userAccountService.GetAccounts(page, pageSize, name, filter, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", accounts));
        }

        [Authorize(Roles = "owner")]
        [HttpPost("accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountViewModel account)
        {
            await _userAccountService.CreateAccount(account);

            return StatusCode(200, _response.Status(200, true, "Created Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("accounts/{id}/role")]
        public async Task<IActionResult> UpdateAccountRole(int id, [FromBody] UpdateAccountRoleViewModel account)
        {
            await _userAccountService.UpdateAccountRole(id, account);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        /// <summary>
        /// Soft-deletes an account (owner only). The first account and the caller's own account can't be deleted.
        /// </summary>
        [Authorize(Roles = "owner")]
        [HttpDelete("accounts/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            await _userAccountService.DeleteAccount(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }

        [Authorize]
        [HttpPatch("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileViewModel profile)
        {
            await _userAccountService.UpdateProfile(CurrentUserId(), profile);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [Authorize]
        [HttpGet("me/credentials")]
        public async Task<IActionResult> GetCredentials()
        {
            var credentials = await _userAccountService.GetCredentials(CurrentUserId());

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", credentials));
        }

        [Authorize]
        [HttpPatch("me/credentials")]
        public async Task<IActionResult> UpdateCredentials([FromBody] UpdateCredentialsViewModel credentials)
        {
            await _userAccountService.UpdateCredentials(CurrentUserId(), credentials);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }
    }
}
