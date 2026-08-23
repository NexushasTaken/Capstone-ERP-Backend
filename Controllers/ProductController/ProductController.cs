using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ProductController
{
    [ApiController]
    [Route("api/Product")]
    public class ProductController(IProductService _productService, ResponseHelper _response) : ControllerBase
    {
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProduct([FromQuery]int page = 1, [FromQuery]int pageSize = 10, [FromQuery]string? name = "")
        {
            var products = await _productService.GetProducts(page, pageSize, name);

            return StatusCode(200, _response.Status(200, true, "Successfully Retrieved Product", products));
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertProduct([FromBody] ProductPostViewModel product)
        {
            await _productService.InsertProduct(product);

            return StatusCode(200, _response.Status(200, true, "Successfully Insert Product", null));
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProduct([FromQuery]int id)
        {
            await _productService.DeleteProduct(id);

            return StatusCode(200, _response.Status(200, true, "Successfully Deleted Product", null));
        }

        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateViewModel product)
        {
            await _productService.UpdateProduct(product);

            return StatusCode(200, _response.Status(200, true, "Successfully Update Product", null));
        }

        [HttpGet("productWithNoCategory")]
        public async Task<IActionResult> ProductWithNoCategory()
        {
            var products = await _productService.ProductWithNoCategory();

            return StatusCode(200, _response.Status(200, true, "Successfully Retroeved Product", products));
        }
    }
}
