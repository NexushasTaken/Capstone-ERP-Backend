using ERP.Repository.Interface.Sales;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.SaleController
{
    [ApiController]
    [Route("api/Sale")]
    public class SaleController(ISaleService _sales) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSales([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? name = "", [FromQuery] int filter = 0, [FromQuery] int orderTypeId = 0, CancellationToken cancellationToken = default)
        {
            var salesData = await _sales.GetSales(page, pageSize, name, filter, orderTypeId, cancellationToken);

            return StatusCode(200, new { status = 200, success = true, message = "Retrieved Successfully", salesData });
        }
    }
}
