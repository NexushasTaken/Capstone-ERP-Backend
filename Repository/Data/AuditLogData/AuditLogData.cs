using ERP.Repository.Configuration.Enum;
using ERP.Repository.Interface.Data.AuditLogData;
using ERP.Repository.Model.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.AuditLogData
{
    public class AuditLogData(DatabaseContext _context) : BaseData(_context), IAuditLogData
    {
        /// <summary>
        /// Adds the log to the context without saving, so it is committed by the next SaveChanges of the action being logged.
        /// </summary>
        public void Add(AuditLog log)
        {
            _context.Add(log);
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsWithoutTracking(
            int page,
            int pageSize,
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        )
        {
            var logs = await FilteringQuery(BaseQuery<AuditLog>(false), userId, action, module, role)
                .Include(l => l.UserAccount)
                    .ThenInclude(u => u!.UserInformation)
                .Include(l => l.UserAccount)
                    .ThenInclude(u => u!.UserRole)
                .OrderByDescending(l => l.Created_At)
                .ThenByDescending(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellation);

            return logs;
        }

        public async Task<int> AuditLogCount(
            int userId,
            int action,
            int module,
            string? role,
            CancellationToken cancellation = default
        )
        {
            var count = await FilteringQuery(BaseQuery<AuditLog>(false), userId, action, module, role)
                .CountAsync(cancellation);

            return count;
        }

        private static IQueryable<AuditLog> FilteringQuery(
            IQueryable<AuditLog> query,
            int userId,
            int action,
            int module,
            string? role
        )
        {
            if (userId > 0)
            {
                query = query.Where(l => l.UserAccountId == userId);
            }

            if (action > 0)
            {
                var actionEnum = (AuditActionEnum)action;
                query = query.Where(l => l.Action == actionEnum);
            }

            if (module > 0)
            {
                var moduleEnum = (AuditModuleEnum)module;
                query = query.Where(l => l.Module == moduleEnum);
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(l => l.UserAccount!.UserRole!.Role == role);
            }

            return query;
        }
    }
}
