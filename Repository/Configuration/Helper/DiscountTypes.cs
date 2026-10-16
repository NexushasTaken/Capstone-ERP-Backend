namespace ERP.Repository.Configuration.Helper
{
    // Order discount kinds, stored lowercase in Orders.DiscountType.
    public static class DiscountTypes
    {
        public const string None = "none";
        public const string Senior = "senior";
        public const string Pwd = "pwd";
        public const string SoloParent = "solo_parent";
        public const string Custom = "custom";

        public static readonly string[] All = [None, Senior, Pwd, SoloParent, Custom];

        // Fixed rates from Philippine law: RA 9994 (senior citizens), RA 10754 (PWDs), RA 11861 (solo parents).
        public static readonly IReadOnlyDictionary<string, decimal> PresetRates = new Dictionary<string, decimal>
        {
            [Senior] = 20m,
            [Pwd] = 20m,
            [SoloParent] = 10m,
        };

        public static bool IsPreset(string? type) => type != null && PresetRates.ContainsKey(type);

        public static string Label(string? type) =>
            type switch
            {
                Senior => "senior citizen",
                Pwd => "PWD",
                SoloParent => "solo parent",
                Custom => "custom",
                _ => "none",
            };
    }
}
