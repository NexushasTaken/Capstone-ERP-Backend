using ERP.Repository.Configuration.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ViewController
{
    [ApiController]
    [Route("api/View")]
    public class ViewController(ResponseHelper _response) : ControllerBase
    {
        [Authorize]
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return StatusCode(200, _response.Status(200, true, "Authorize", null));
        }

    }
}
