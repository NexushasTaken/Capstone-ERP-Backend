using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.DashboardController
{
    [ApiController]
    [Route("api/Dashboard")]
    public class DashboardController(IDashboardService _dashboard, ResponseHelper _response) : ControllerBase
    {
        [HttpGet("SalesOverView")]
        public async Task<IActionResult> SalesOverView(DateTime from, DateTime to, CancellationToken cancellation = default)
        {
            var data = await _dashboard.SalesOverView(from,to,cancellation);

            return StatusCode(200, _response.Status(200, true, "Successfully", data));
        }
    }
}
