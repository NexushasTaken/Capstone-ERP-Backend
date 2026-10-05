using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ERP.Controllers.InventoryController
{
    [ApiController]
    [Route("api/Inventory")]
    public class InventoryController(IInventoryService _inventory, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// Get all inventory with filtering
        /// </summary>
        /// <param name="page">asdasd</param>
        /// <param name="pageSize"></param>
        /// <param name="name">case-insensitive search on name, warehouse, status; a number or INV-n also matches the Id</param>
        /// <param name="filter">Filtering Numbering works like this: 1: Name A - Z, 2: Name Z - A, 3: Quantity High - Low, 4: Quantity Low - High, 5: Reorder Point Ascending, 6: Warehouse Id Ascending</param>
        /// <param name="cancellation"></param>
        /// <param name="statusId"></param>
        /// <param name="wareHousePresent">0 to include all items that has warehouse otherewise use 1</param>
        /// <param name="warehouseId">only items in this warehouse; 0 for any</param>
        /// <param name="categoryId">only items whose product is in this category; 0 for any</param>
        /// <param name="minQuantity">minimum quantity, inclusive</param>
        /// <param name="maxQuantity">maximum quantity, inclusive; 0 lists out-of-stock items</param>
        /// <param name="dateFrom">date arrived on or after this day</param>
        /// <param name="dateTo">date arrived on or before this day</param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetInventory([FromQuery] int page = 1, [FromQuery] int pageSize = 10,[FromQuery] string name = "", [FromQuery] int filter = 0, [FromQuery] int statusId = 0,[FromQuery] int wareHousePresent = 0, [FromQuery] int warehouseId = 0, [FromQuery] int categoryId = 0, [FromQuery] int? minQuantity = null, [FromQuery] int? maxQuantity = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null, CancellationToken cancellation = default)
        {
            var listFilter = new InventoryListFilter(warehouseId, categoryId, minQuantity, maxQuantity, dateFrom, dateTo);

            var inventories = await _inventory.GetInventories(page,pageSize,name,filter,statusId,wareHousePresent,listFilter,cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventories));
        }

        /// <summary>
        /// Retrieves one page of warehouses.
        /// </summary>
        /// <param name="name">case-insensitive search on the warehouse name or address; a number also matches the Id</param>
        /// <param name="filter">sort: 0 newest first, 1 Id, 2 name A - Z, 3 name Z - A</param>
        [HttpGet("warehouse/all")]
        public async Task<IActionResult> GetWareHouses([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? name = "", [FromQuery] int filter = 0, CancellationToken cancellation = default)
        {
            var wareHouse = await _inventory.GetWarehouses(page, pageSize, name, filter, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", wareHouse));
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

        [HttpGet("movement/velocity")]
        public async Task<IActionResult> GetMovementVelocity([FromQuery] int cutOffDate = 7, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var inventory = await _inventory.GetMovementVelocity(cutOffDate, page, pageSize);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventory));
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
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] InventoryPostViewModel inventory)
        {
            await _inventory.InsertItem(inventory);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }

        [HttpPost("warehouse/insert")]
        public async Task<IActionResult> InsertWareHouse([FromBody] InventoryWareHousePostViewModel wareHouse)
        {
            await _inventory.NewWareHouse(wareHouse);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }


        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateInventory([FromBody] InventoryUpdateViewModel inventory)
        {
            await _inventory.UpdateInventory(inventory);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [HttpPatch("markasdamage")]
        public async Task<IActionResult> MarkAsDamaged([FromBody]InventoryDamagePostViewModel damaged)
        {
            await _inventory.MarkAsDamaged(damaged);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [HttpPatch("warehouse/patch")]
        public async Task<IActionResult> UpdateWareHouse([FromBody]InventoryWareHouseUpdateViewModel wareHouse)
        {
            await _inventory.UpdateWareHouse(wareHouse);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [HttpPatch("restock")]
        public async Task<IActionResult> RestockInventory(InventoryRestockViewModel inventory)
        {
            await _inventory.Restock(inventory);

            return StatusCode(200, _response.Status(200, true, "Restocked Successfully", null));
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            await _inventory.DeleteItem(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }

        [HttpDelete("warehouse/delete")]
        public async Task<IActionResult> DeleteWareHouse([FromQuery] int id)
        {
            await _inventory.DeleteWareHouse(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }
    }
}
