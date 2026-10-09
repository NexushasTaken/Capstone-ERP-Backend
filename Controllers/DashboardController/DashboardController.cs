using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Dashboard;
using ERP.Repository.Interface.SSA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.DashboardController
{
    [ApiController]
    [Authorize]
    [Route("api/Dashboard")]
    public class DashboardController(IDashboardService _dashboard, ResponseHelper _response, IForecastService _forecast)
        : ControllerBase
    {
        [HttpGet("sales")]
        public async Task<IActionResult> SalesOverView(
            DateTime from,
            DateTime to,
            CancellationToken cancellation = default
        )
        {
            var data = await _dashboard.SalesOverView(from, to, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> InventoryOverView()
        {
            var data = await _dashboard.InventoryOverView();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }

        [HttpGet("inventory/forecast")]
        public async Task<IActionResult> Forecast(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool forceForecast = false,
            [FromQuery] string? search = null,
            [FromQuery] bool needOrderOnly = false
        )
        {
            var data = await _forecast.GetLatestForecast(page, pageSize, forceForecast, search, needOrderOnly);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }

        // One product's weekly demand history and forecast, for the demand chart
        [HttpGet("inventory/forecast/{productId:int}")]
        public async Task<IActionResult> DemandChart(int productId)
        {
            var data = await _forecast.GetDemandChart(productId);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }
    }
}
