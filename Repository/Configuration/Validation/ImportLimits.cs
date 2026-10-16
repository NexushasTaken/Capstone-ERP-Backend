namespace ERP.Repository.Configuration.Validation
{
    public static class ImportLimits
    {
        // Largest file a single import accepts; the whole batch is validated and saved in one request.
        public const int MaxRows = 1000;

        public const string EmptyMessage = "The file has no rows to import";
        public static readonly string TooManyMessage = $"A file can have at most {MaxRows} rows";
    }
}
