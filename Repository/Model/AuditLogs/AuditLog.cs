using ERP.Repository.Configuration.Enum;
using ERP.Repository.Model.UserAccounts;

namespace ERP.Repository.Model.AuditLogs
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int? UserAccountId { get; set; }
        public UserAccount? UserAccount { get; set; }
        public AuditModuleEnum Module { get; set; }
        public AuditActionEnum Action { get; set; }
        public int? EntityId { get; set; }
        public string? Message { get; set; }
        public DateTime Created_At { get; set; }
    }
}
