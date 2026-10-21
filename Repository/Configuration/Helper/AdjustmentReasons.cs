using ERP.Repository.Configuration.Enum;

namespace ERP.Repository.Configuration.Helper
{
    // Display names for AdjustmentReasonEnum (lowercase, like the lookup values)
    public static class AdjustmentReasons
    {
        public static string Label(int reasonId) =>
            (AdjustmentReasonEnum)reasonId switch
            {
                AdjustmentReasonEnum.Damaged => "damaged",
                AdjustmentReasonEnum.Lost => "lost",
                AdjustmentReasonEnum.Found => "found",
                AdjustmentReasonEnum.CountCorrection => "count correction",
                AdjustmentReasonEnum.OpeningStock => "opening stock",
                _ => "unknown",
            };
    }
}
