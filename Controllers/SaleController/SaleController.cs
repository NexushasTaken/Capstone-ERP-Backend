using ERP.Repository.Interface.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.SaleController
{
    [ApiController]
    [Authorize(Roles = "owner")]
    [Route("api/Sale")]
    public class SaleController(ISaleService _sales) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllSales(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = "",
            [FromQuery] int filter = 0,
            [FromQuery] int orderTypeId = 0,
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            CancellationToken cancellationToken = default
        )
        {
            var salesData = await _sales.GetSales(
                page,
                pageSize,
                name,
                filter,
                orderTypeId,
                from,
                to,
                cancellationToken
            );

            return StatusCode(
                200,
                new
                {
                    status = 200,
                    success = true,
                    message = "Retrieved Successfully",
                    salesData,
                }
            );
        }

        [HttpGet("range")]
        public async Task<IActionResult> GetSaleDateRange(CancellationToken cancellationToken = default)
        {
            var range = await _sales.GetSaleDateRange(cancellationToken);

            return StatusCode(
                200,
                new
                {
                    status = 200,
                    success = true,
                    message = "Retrieved Successfully",
                    range,
                }
            );
        }

        // period: daily (the day), weekly (its Monday-to-Sunday week) or monthly (its month).
        // date is a Philippine day and defaults to today.
        [HttpGet("report")]
        public async Task<IActionResult> GetSalesReport(
            [FromQuery] string? period = "daily",
            [FromQuery] DateOnly? date = null,
            CancellationToken cancellationToken = default
        )
        {
            var report = await _sales.GetSalesReportPdf(period, date, cancellationToken);

            return File(report.Content, "application/pdf", report.FileName);
        }
    }
}
