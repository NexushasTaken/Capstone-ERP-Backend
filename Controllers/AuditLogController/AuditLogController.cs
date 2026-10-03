using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers.AuditLogController
{
    [ApiController]
    [Route("api/AuditLog")]
    public class AuditLogController(IAuditLogService _auditLog, ResponseHelper _response) : ControllerBase
    {
        /// <summary>
        /// Retrieves audit logs sorted from newest to oldest.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="userId">filter by the user who performed the action, 0 for all users</param>
        /// <param name="action">1 Create, 2 Update, 3 Delete, 4 StatusChange, 5 IncreaseStock, 6 ReturnStock, 7 CurrentItemDamage, 8 ReturnItemDamage, 0 for all actions</param>
        /// <param name="module">1 Category, 2 Product, 3 Driver, 4 Order, 5 Inventory, 6 Warehouse, 0 for all modules</param>
        /// <param name="role">filter by the role of the user who performed the action ("owner" or "secretary"), empty for all roles</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int userId = 0, [FromQuery] int action = 0, [FromQuery] int module = 0, [FromQuery] string? role = null, CancellationToken cancellation = default)
        {
            var logs = await _auditLog.GetLogs(page, pageSize, userId, action, module, role, cancellation);

            return StatusCode(200, _response.Status(200, true, "Retrieved Successfully", logs));
        }
    }
}
