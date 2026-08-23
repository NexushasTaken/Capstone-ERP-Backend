using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Mvc;

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
        /// <param name="searchString">Search for Name</param>
        /// <param name="filter">Filtering Numbering works like this: 1: Name A - Z, 2: Name Z - A, 3: Quantity High - Low, 4: Quantity Low - High, 5: Reorder Point Ascending, 6: Warehouse Id Ascending</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetInventory([FromQuery] int page = 1, [FromQuery] int pageSize = 10,[FromQuery] string searchString = "", [FromQuery] int filter = 0, [FromQuery] int statusId = 0, CancellationToken cancellation = default)
        {
            var inventories = await _inventory.GetInventories(page,pageSize,searchString,filter,statusId,cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", inventories));
        }

        /// <summary>
        /// Insert new inventory item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="id">Inventory Id, if you pass any, means we will just add the quantity to the existing item</param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> Insert([FromBody] InventoryPostViewModel inventory, [FromQuery] int id = 0)
        {
            await _inventory.InsertItem(inventory, id);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            await _inventory.DeleteItem(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }

        /// <summary>
        /// This is for getting how many inventory specific status have
        /// </summary>
        /// <param name="id">Need to insert status id here</param>
        /// <returns></returns>
        [HttpGet("statusCount")]
        public async Task<IActionResult> StatusCount([FromQuery] int id)
        {
            var count = await _inventory.StatusInventoryCount(id);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", count));
        }
    }
}
