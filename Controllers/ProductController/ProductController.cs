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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="name"></param>
        /// <param name="categoryPresent">0 to include only the products that has category otherwise 1</param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProduct([FromQuery]int page = 1, [FromQuery]int pageSize = 10, [FromQuery]string? name = "", [FromQuery] int categoryPresent = 0)
        {
            var products = await _productService.GetProducts(page, pageSize, name, categoryPresent);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", products));
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertProduct([FromBody] ProductPostViewModel product)
        {
            await _productService.InsertProduct(product);

            return StatusCode(200, _response.Status(200, true, "Added Successfully", null));
        }

        [HttpPatch("patch")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductUpdateViewModel product)
        {
            await _productService.UpdateProduct(product);

            return StatusCode(200, _response.Status(200, true, "Updated ", null));
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProduct([FromQuery] int id)
        {
            await _productService.DeleteProduct(id);

            return StatusCode(200, _response.Status(200, true, "Deleted Successfully", null));
        }
    }
}
