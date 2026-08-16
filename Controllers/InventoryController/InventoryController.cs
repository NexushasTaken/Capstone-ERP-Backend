using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.InventoryController
{
    [ApiController]
    [Route("api/Inventory")]
    public class InventoryController : ControllerBase
    {
        
        [HttpGet("getInventory")]
        public async Task<IActionResult> GetInventory()
        {
            return Ok("Inventory Controller is working");
        }
    }
}
