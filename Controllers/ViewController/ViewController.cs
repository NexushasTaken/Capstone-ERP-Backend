using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ViewController
{
    [ApiController]
    [Route("api/View")]
    public class ViewController : ControllerBase
    {
        [Authorize]
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return StatusCode(200, new { message = "Authorized" });
        }

    }
}
