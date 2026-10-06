using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.CurrentUser;
using ERP.Repository.Interface.Data.AuditLogData;
using ERP.Repository.Model.AuditLogs;
using ERP.Repository.ViewModel.AuditLogs;

namespace ERP.Repository.Services.AuditLogs
{
    public class AuditLogService(IAuditLogData _auditLog, ICurrentUserService _currentUser) : IAuditLogService
    {
        public int? CurrentUserId => _currentUser.UserId;

        /// <summary>
        /// Queues an audit log entry. It is saved together with the caller's next SaveChanges.
        /// </summary>
        public void Log(
            AuditModuleEnum module,
            AuditActionEnum action,
            string message,
            int? entityId,
            DateTime timestamp
        )
        {
            _auditLog.Add(
                new AuditLog
                {
                    UserAccountId = _currentUser.UserId,
                    Module = module,
                    Action = action,
                    EntityId = entityId,
                    Message = message,
                    Created_At = timestamp,
                }
            );
        }

        public async Task<AuditLogPageViewModel> GetLogs(
            int page,
            int pageSize,
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            if (action != 0 && !Enum.IsDefined(typeof(AuditActionEnum), action))
            {
                throw new BadRequest("Invalid action filter");
            }

            if (module != 0 && !Enum.IsDefined(typeof(AuditModuleEnum), module))
            {
                throw new BadRequest("Invalid module filter");
            }

            var logs = await _auditLog.GetAuditLogsWithoutTracking(
                page,
                pageSize,
                userId,
                action,
                module,
                role,
                cancellation
            );
            var count = await _auditLog.AuditLogCount(userId, action, module, role, cancellation);

            var final = logs.Select(l => new AuditLogViewModel
            {
                Id = l.Id,
                Module = l.Module.ToString(),
                Action = l.Action.ToString(),
                Message = l.Message,
                EntityId = l.EntityId,
                UserAccountId = l.UserAccountId,
                UserFullName =
                    l.UserAccount?.UserInformation == null
                        ? "Unknown"
                        : $"{l.UserAccount.UserInformation.FirstName} {l.UserAccount.UserInformation.LastName}".Trim(),
                UserRole = l.UserAccount?.UserRole?.Role,
                Created_At = l.Created_At,
            });

            return new AuditLogPageViewModel
            {
                Logs = final,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };
        }
    }
}
