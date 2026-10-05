namespace ERP.Repository.Configuration.Helper
{
    public static class SearchPattern
    {
        /// <summary>
        /// Builds an ILIKE pattern that matches values containing <paramref name="text"/>.
        /// Use with EF.Functions.ILike so search is case-insensitive on Postgres.
        /// %, _ and \ typed by the user are escaped so they match literally.
        /// </summary>
        public static string Contains(string text)
        {
            var escaped = text.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");

            return $"%{escaped}%";
        }
    }
}
