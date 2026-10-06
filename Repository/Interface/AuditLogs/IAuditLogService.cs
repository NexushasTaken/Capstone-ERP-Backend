using ERP.Repository.Configuration.Enum;
using ERP.Repository.ViewModel.AuditLogs;

namespace ERP.Repository.Interface.AuditLogs
{
    public interface IAuditLogService
    {
        int? CurrentUserId { get; }
        void Log(AuditModuleEnum module, AuditActionEnum action, string message, int? entityId, DateTime timestamp);
        Task<AuditLogPageViewModel> GetLogs(
            int page,
            int pageSize,
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        );
    }
}
