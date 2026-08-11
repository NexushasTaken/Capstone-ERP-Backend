namespace ERP.Repository.Configuration
{
    public class HttpContextSetting
    {
        public bool InDevelopment { get; set; }
        public bool IsHttpOnly { get; set; }
        public string? SameSite { get; set; }
        public int ExpireInDays { get; set; }
    }
}
