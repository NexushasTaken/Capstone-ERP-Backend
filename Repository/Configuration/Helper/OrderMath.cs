namespace ERP.Repository.Configuration.Helper
{
    public static class OrderMath
    {
        public static decimal DiscountAmount(decimal subtotal, decimal discountPercent)
        {
            return Math.Round(subtotal * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);
        }
    }
}
