using ERP.Repository.Configuration.Enum;

namespace ERP.Repository.Configuration.Helper
{
    public class ReorderRatio
    {
        public static int Ratio(int quantity, int reorderPoint)
        {
            double ratio = (double)quantity / reorderPoint;

            if (ratio >= 2.0)
            {
                return (int)InventoryStatusEnum.Available;
            }
            else if (ratio >= 1.0)
            {
                return (int)InventoryStatusEnum.LowStock;
            }
            return (int)InventoryStatusEnum.Critical;
        }
    }
}
