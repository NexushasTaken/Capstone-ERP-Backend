using ERP.Repository.Model.AuditLogs;

namespace ERP.Repository.Interface.Data.AuditLogData
{
    public interface IAuditLogData
    {
        void Add(AuditLog log);
        Task<IEnumerable<AuditLog>> GetAuditLogsWithoutTracking(
            int page,
            int pageSize,
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        );
        Task<int> AuditLogCount(
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        );
    }
}
