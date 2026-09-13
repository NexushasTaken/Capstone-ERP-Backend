using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.OrderData;
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

        [HttpGet("types")]
        public async Task<IActionResult> GetOrderTypes()
        {
            var types = await _orderService.GetOrderTypes();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", types));
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetOrderStatuses()
        {
            var statuses = await _orderService.GetOrderStatuses();
            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", statuses));
        }

        [HttpGet("riders")]
        public async Task<IActionResult> GetDeliveryRiders()
        {
            var riders = await _orderService.GetDeliveryRiders();
            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", riders));
        }

        [HttpGet("status/count")]
        public async Task<IActionResult> StatusCount()
        {
            var data = await _orderService.OrdersStatusCount();

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", data));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="name"></param>
        /// <param name="filter">input 1 to sort first and last name to A - Z, 2 to sort Z - A</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("driver/all")]
        public async Task<IActionResult> GetDrivers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? name = "", [FromQuery] int filter = 0, CancellationToken cancellationToken = default)
        {
            var drivers = await _orderService.GetDrivers(page, pageSize, name, filter, cancellationToken);
            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", drivers));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Make sure to put List of orders even tho you only have a single order</param>
        /// <returns></returns>
        [HttpPost("insert")]
        public async Task<IActionResult> AddNewOrder([FromBody] OrderPostViewModel order)
        {
            var message = await _orderService.InsertOrder(order);

            return StatusCode(200, _response.Status(200, true, "Orders Added Successfully", message));
        }

        [HttpPost("driver/insert")]
        public async Task<IActionResult> AddNewDriver([FromBody] DeliveryDriverPostViewModel driver)
        {
            await _orderService.AddDriver(driver);

            return StatusCode(200, _response.Status(200, true, "Driver Added Successfully", null));
        }

        [HttpPatch("status/patch")]
        public async Task<IActionResult> PatchOrderStatus(OrderStatusPostViewModel status)
        {
            await _orderService.UpdateOrderStatus(status);

            return StatusCode(200, _response.Status(200, true, "Order Status Updated Successfully", null));
        }

        [HttpPut("driver/put")]
        public async Task<IActionResult> PatchDriver([FromBody] DeliveryDriverPatchViewModel driver)
        {
            await _orderService.UpdateDriver(driver);

            return StatusCode(200, _response.Status(200, true, "Driver Updated Successfully", null));
        }

        [HttpDelete("driver/delete")]
        public async Task<IActionResult> DeleteDriver([FromQuery] int id)
        {
            await _orderService.DeleteDriver(id);

            return StatusCode(200, _response.Status(200, true, "Driver Deleted Successfully", null));
        }
    }
}
