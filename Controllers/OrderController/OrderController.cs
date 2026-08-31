using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Orders;
using ERP.Repository.Model.Orders;
using ERP.Repository.ViewModel.Orders;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.OrderController
{
    [ApiController]
    [Route("api/Order")]
    public class OrderController(IOrderService _orderService, ResponseHelper _response) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders(int page = 1, int pageSize = 10, string? name = "", int filter = 0, int statusId = 0, int orderTypeId = 0, CancellationToken cancellationToken = default)
        {
            var orders = await _orderService.GetOrders(page,pageSize,name,filter,statusId,orderTypeId,cancellationToken);

            return StatusCode(200, _response.Status(200,true,"Retrieved Successfully", orders));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Make sure to put List of orders even tho you only have a single order</param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> AddNewOrder([FromBody] List<OrderPostViewModel> order)
        {
            await _orderService.InsertOrder(order);

            return StatusCode(200, _response.Status(200, true, "Orders Added Successfully", null));
        }
    }
}
