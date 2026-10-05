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

        /// <summary>
        /// Reads an id out of search text. Accepts a bare number ("12") or the prefixed form
        /// shown in the list view ("PR-12"), case-insensitive.
        /// </summary>
        public static bool TryParseId(string text, string prefix, out int id)
        {
            var value = text.Trim();

            if (value.StartsWith(prefix + "-", StringComparison.OrdinalIgnoreCase))
            {
                value = value[(prefix.Length + 1)..].Trim();
            }

            return int.TryParse(value, out id);
        }
    }
}
