using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Purchases;
using ERP.Repository.ViewModel.Purchases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.PurchaseController
{
    // Purchases are the owner's: buying stock from suppliers.
    [ApiController]
    [Authorize(Roles = "owner")]
    [Route("api/Purchase")]
    public class PurchaseController(IPurchaseService _purchases, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// One page of purchases with their lines
        /// </summary>
        /// <param name="name">case-insensitive search on supplier and product names; PO-12 or 12 also matches the Id</param>
        /// <param name="statusId">1 ordered, 2 received, 3 cancelled; 0 for all</param>
        /// <param name="filter">sort: 0 newest order date first, 1 oldest first</param>
        [HttpGet("all")]
        public async Task<IActionResult> GetPurchases(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = "",
            [FromQuery] int statusId = 0,
            [FromQuery] int filter = 0,
            CancellationToken cancellation = default
        )
        {
            var purchases = await _purchases.GetPurchases(page, pageSize, name, statusId, filter, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", purchases));
        }

        [HttpGet("status/count")]
        public async Task<IActionResult> StatusCount()
        {
            var counts = await _purchases.StatusCount();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", counts));
        }

        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] PurchasePostViewModel purchase)
        {
            await _purchases.InsertPurchase(purchase);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }

        /// <summary>
        /// Edit an ordered purchase; its lines are replaced by the ones sent
        /// </summary>
        [HttpPut("put")]
        public async Task<IActionResult> Update([FromBody] PurchasePutViewModel purchase)
        {
            await _purchases.UpdatePurchase(purchase);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [HttpPatch("cancel")]
        public async Task<IActionResult> Cancel([FromQuery] int id)
        {
            await _purchases.CancelPurchase(id);

            return StatusCode(200, _response.Status(200, true, "Cancelled Successfully", null));
        }

        /// <summary>
        /// Receive an ordered purchase: lines = every product with how many arrived (0 up to the ordered quantity)
        /// </summary>
        [HttpPatch("receive")]
        public async Task<IActionResult> Receive([FromBody] PurchaseReceiveViewModel receive)
        {
            await _purchases.ReceivePurchase(receive);

            return StatusCode(200, _response.Status(200, true, "Received Successfully", null));
        }
    }
}
