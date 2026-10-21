using ERP.Repository.Interface.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ReportController
{
    // PDF reports for the owner. The Sales report stays at GET /api/Sale/report.
    [ApiController]
    [Authorize(Roles = "owner")]
    [Route("api/Report")]
    public class ReportController(IReportService _reports) : ControllerBase
    {
        /// <summary>
        /// Stock on hand right now: every stock record, critical first
        /// </summary>
        [HttpGet("stock")]
        public async Task<IActionResult> Stock(CancellationToken cancellation = default)
        {
            var report = await _reports.StockReportPdf(cancellation);

            return File(report.Content, "application/pdf", report.FileName);
        }

        /// <summary>
        /// Purchases received in the period
        /// </summary>
        /// <param name="period">daily (the day), weekly (its Monday-to-Sunday week) or monthly (its month)</param>
        /// <param name="date">a Philippine day; defaults to today</param>
        [HttpGet("purchases")]
        public async Task<IActionResult> Purchases(
            [FromQuery] string? period = "daily",
            [FromQuery] DateOnly? date = null,
            CancellationToken cancellation = default
        )
        {
            var report = await _reports.PurchaseReportPdf(period, date, cancellation);

            return File(report.Content, "application/pdf", report.FileName);
        }

        /// <summary>
        /// Stock adjustments made in the period, by reason
        /// </summary>
        /// <param name="period">daily (the day), weekly (its Monday-to-Sunday week) or monthly (its month)</param>
        /// <param name="date">a Philippine day; defaults to today</param>
        [HttpGet("adjustments")]
        public async Task<IActionResult> Adjustments(
            [FromQuery] string? period = "daily",
            [FromQuery] DateOnly? date = null,
            CancellationToken cancellation = default
        )
        {
            var report = await _reports.AdjustmentReportPdf(period, date, cancellation);

            return File(report.Content, "application/pdf", report.FileName);
        }
    }
}
