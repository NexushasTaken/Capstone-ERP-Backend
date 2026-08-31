using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;

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
        /// <param name="name">Search for Name</param>
        /// <param name="filter">Filtering Numbering works like this: 1: Name A - Z, 2: Name Z - A, 3: Quantity High - Low, 4: Quantity Low - High, 5: Reorder Point Ascending, 6: Warehouse Id Ascending</param>
        /// <param name="cancellation"></param>
        /// <param name="statusId"></param>
        /// <param name="wareHousePresent">0 to include all items that has warehouse otherewise use 1</param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetInventory([FromQuery] int page = 1, [FromQuery] int pageSize = 10,[FromQuery] string name = "", [FromQuery] int filter = 0, [FromQuery] int statusId = 0,[FromQuery] int wareHousePresent = 0, CancellationToken cancellation = default)
        {
            var inventories = await _inventory.GetInventories(page,pageSize,name,filter,statusId,wareHousePresent,cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventories));
        }

        [HttpGet("warehouse/all")]
        public async Task<IActionResult> GetWareHouses()
        {
            var wareHouse = await _inventory.GetWarehouses();

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
        public async Task<IActionResult> GetMovementVelocity([FromQuery] int cutOffDate)
        {
            var inventory = await _inventory.GetMovementVelocity(cutOffDate);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventory));
        }

        /// <summary>
        /// Insert new inventory item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="id">Inventory Id, if you pass any, means we will just add the quantity to the existing item. This required only the quantity to be filled in payload :)</param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] InventoryPostViewModel inventory, [FromQuery] int id = 0)
        {
            await _inventory.InsertItem(inventory, id);

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
