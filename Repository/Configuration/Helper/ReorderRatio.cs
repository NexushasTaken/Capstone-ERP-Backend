namespace ERP.Repository.Configuration.Helper
{
    public class ReorderRatio
    {
        public static int Ratio(int quantity, int reorderPoint)
        {
            double ratio = (double)quantity / reorderPoint;

            if (ratio >= 2.0)
            {
                return 1;
            }
            else if (ratio >= 1.0)
            {
                return 2;
            }
            return 3;
        }
    }
}
