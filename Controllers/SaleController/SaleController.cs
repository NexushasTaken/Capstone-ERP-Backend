using ERP.Repository.Interface.Sales;
using ERP.Repository.ViewModel.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.SaleController
{
    [ApiController]
    [Authorize(Roles = "owner,secretary")]
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

        [HttpGet("returns")]
        public async Task<IActionResult> GetReturns(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = "",
            [FromQuery] DateOnly? from = null,
            [FromQuery] DateOnly? to = null,
            CancellationToken cancellationToken = default
        )
        {
            var returnsData = await _sales.GetReturns(page, pageSize, name, from, to, cancellationToken);

            return StatusCode(
                200,
                new
                {
                    status = 200,
                    success = true,
                    message = "Retrieved Successfully",
                    returnsData,
                }
            );
        }

        [HttpPost("return")]
        public async Task<IActionResult> ReturnItems([FromBody] SaleReturnPostViewModel saleReturn)
        {
            await _sales.ReturnItems(saleReturn);

            return StatusCode(
                200,
                new
                {
                    status = 200,
                    success = true,
                    message = "Return recorded",
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
        [Authorize(Roles = "owner")]
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
