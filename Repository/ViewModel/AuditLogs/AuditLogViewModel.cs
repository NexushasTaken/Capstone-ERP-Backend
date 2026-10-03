namespace ERP.Repository.ViewModel.AuditLogs
{
    public class AuditLogViewModel
    {
        public int Id { get; set; }
        public string? Module { get; set; }
        public string? Action { get; set; }
        public string? Message { get; set; }
        public int? EntityId { get; set; }
        public int? UserAccountId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserRole { get; set; }
        public DateTime Created_At { get; set; }
    }

    public class AuditLogPageViewModel
    {
        public IEnumerable<AuditLogViewModel> Logs { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }
}
