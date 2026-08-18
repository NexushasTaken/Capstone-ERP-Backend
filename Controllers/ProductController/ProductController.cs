using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.ProductController
{
    [ApiController]
    [Route("api/Product")]
    public class ProductController(IProductService _productService, ResponseHelper _response) : ControllerBase
    {
        
        [HttpGet("allProduct")]
        public async Task<IActionResult> GetAllProduct()
        {
            var products = await _productService.GetProducts();

            return StatusCode(200, _response.Status(200, true, "Successfully Retrieved Product", products));
        }
    }
}
