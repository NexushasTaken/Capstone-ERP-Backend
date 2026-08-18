using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Product;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ProductController
{
    [ApiController]
    [Route("api/Category")]
    public class CategoryController(IProductService _productService, ResponseHelper _response) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategory()
        {
            var categories = await _productService.GetCategories();

            return StatusCode(200, _response.Status(200, true, "Successfully Retrieved Category", categories));
        }


        [HttpPost("newCategory")]
        public async Task<IActionResult> InsertNewCategory(string categoryName)
        {
            await _productService.InsertCategory(categoryName);

            return StatusCode(200, _response.Status(200, true, "Insert Successfully", null));
        }
    }
}
