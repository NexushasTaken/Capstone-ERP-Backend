using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Products;
using ERP.Repository.ViewModel.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ProductController
{
    [ApiController]
    [Authorize]
    [Route("api/Category")]
    public class CategoryController(IProductService _productService, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// Retrieves one page of categories.
        /// </summary>
        /// <param name="name">case-insensitive search on the category type; a number also matches the Id</param>
        /// <param name="filter">sort: 0 newest first, 1 Id, 2 type A - Z, 3 type Z - A</param>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? name = "",
            [FromQuery] int filter = 0,
            CancellationToken cancellation = default
        )
        {
            var categories = await _productService.GetCategories(page, pageSize, name, filter, cancellation);

            return StatusCode(200, _response.Status(200, true, "Successfully Retrieved Category", categories));
        }

        [Authorize(Roles = "owner")]
        [HttpPost("insert")]
        public async Task<IActionResult> InsertNewCategory(string categoryName)
        {
            await _productService.InsertCategory(categoryName);

            return StatusCode(200, _response.Status(200, true, "Insert Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateViewModel category)
        {
            await _productService.UpdateCategory(category);

            return StatusCode(200, _response.Status(200, true, "Updated Successfully", null));
        }

        [Authorize(Roles = "owner")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _productService.DeleteCategory(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }
    }
}
