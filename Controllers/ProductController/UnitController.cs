using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Products;
using ERP.Repository.ViewModel.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ProductController
{
    [ApiController]
    [Authorize]
    [Route("api/Unit")]
    public class UnitController(IUnitService _unitService, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// Retrieves one page of units of measurement, each with how many products use it.
        /// </summary>
        /// <param name="name">case-insensitive search on the unit name; a number also matches the Id</param>
        /// <param name="filter">sort: 0 newest first, 1 Id, 2 name A - Z, 3 name Z - A</param>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUnits(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = "",
            [FromQuery] int filter = 0,
            CancellationToken cancellation = default
        )
        {
            var units = await _unitService.GetUnits(page, pageSize, name, filter, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", units));
        }

        [Authorize(Roles = "owner")]
        [HttpPost("insert")]
        public async Task<IActionResult> InsertUnit([FromBody] UnitPostViewModel unit)
        {
            await _unitService.InsertUnit(unit);

            return StatusCode(200, _response.Status(200, true, "Insert Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateUnit([FromBody] UnitUpdateViewModel unit)
        {
            await _unitService.UpdateUnit(unit);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        /// <summary>
        /// Deletes a unit; refused (400) while any product uses it.
        /// </summary>
        [Authorize(Roles = "owner")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUnit([FromQuery] int id)
        {
            await _unitService.DeleteUnit(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }
    }
}
