using System.Reflection.Emit;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ERP.Controllers.InventoryController
{
    [ApiController]
    [Authorize]
    [Route("api/Inventory")]
    public class InventoryController(IInventoryService _inventory, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// Get all inventory with filtering
        /// </summary>
        /// <param name="page">asdasd</param>
        /// <param name="pageSize"></param>
        /// <param name="name">case-insensitive search on product name, status; a number or INV-n also matches the Id</param>
        /// <param name="filter">Filtering Numbering works like this: 1: Product Name A - Z, 2: Product Name Z - A, 3: Quantity High - Low, 4: Quantity Low - High, 5: Reorder Point Ascending</param>
        /// <param name="cancellation"></param>
        /// <param name="statusId"></param>
        /// <param name="categoryId">only items whose product is in this category; 0 for any</param>
        /// <param name="minQuantity">minimum quantity, inclusive</param>
        /// <param name="maxQuantity">maximum quantity, inclusive; 0 lists out-of-stock items</param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetInventory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string name = "",
            [FromQuery] int filter = 0,
            [FromQuery] int statusId = 0,
            [FromQuery] int categoryId = 0,
            [FromQuery] int? minQuantity = null,
            [FromQuery] int? maxQuantity = null,
            CancellationToken cancellation = default
        )
        {
            var listFilter = new InventoryListFilter(categoryId, minQuantity, maxQuantity);

            var inventories = await _inventory.GetInventories(
                page,
                pageSize,
                name,
                filter,
                statusId,
                listFilter,
                cancellation
            );

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventories));
        }

        [HttpGet("inventoryLabel/forInsert")]
        public async Task<IActionResult> GetInventoryLabel()
        {
            var label = await _inventory.GetInventoryLabels();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", label));
        }

        [HttpGet("damage/item")]
        public async Task<IActionResult> GetDamagedItem([FromQuery] int id)
        {
            var damaged = await _inventory.GetDamageInventory(id);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", damaged));
        }

        [HttpGet("movement/item")]
        public async Task<IActionResult> GetItemTransaction([FromQuery] int id)
        {
            var transac = await _inventory.GetInventoryItemTransaction(id);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", transac));
        }

        [HttpGet("status/count")]
        public async Task<IActionResult> GetStatusCount()
        {
            var data = await _inventory.InventoryStatusCount();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }

        [HttpGet("insert/product/all")]
        public async Task<IActionResult> GetAllProductsForInsert()
        {
            var product = await _inventory.GetAllProductForInsert();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", product));
        }

        /// <summary>
        /// Insert new inventory item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="id">Inventory Id, if you pass any, means we will just add the quantity to the existing item. This required only the quantity to be filled in payload :)</param>
        /// <returns></returns>
        [Authorize(Roles = "owner")]
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] InventoryPostViewModel inventory)
        {
            await _inventory.InsertItem(inventory);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateInventory([FromBody] InventoryUpdateViewModel inventory)
        {
            await _inventory.UpdateInventory(inventory);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("markasdamage")]
        public async Task<IActionResult> MarkAsDamaged([FromBody] InventoryDamagePostViewModel damaged)
        {
            await _inventory.MarkAsDamaged(damaged);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("restock")]
        public async Task<IActionResult> RestockInventory(InventoryRestockViewModel inventory)
        {
            await _inventory.Restock(inventory);

            return StatusCode(200, _response.Status(200, true, "Restocked Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            await _inventory.DeleteItem(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }
    }
}
