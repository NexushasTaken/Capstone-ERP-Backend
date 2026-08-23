using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.Inventories;

namespace ERP.Repository.Configuration.Validation
{
    public class InventoryValidation
    {
        public static void ValidateItem(InventoryPostViewModel inventory)
        {
            if (string.IsNullOrWhiteSpace(inventory.Name))
            {
                throw new BadRequest("Item name is required");
            }
            if(inventory.Quantity <= 0)
            {
                throw new BadRequest("Quantity should be higher than 0");
            }
            if (inventory.WarehouseId <= 0)
            {
                throw new BadRequest("Invalid Warehouse ID");
            }
            if(inventory.ProductId <= 0)
            {
                throw new BadRequest("Invalid Product ID");
            }
            if(inventory.ReorderPoint <= 0)
            {
                throw new BadRequest("Reorder Point should be higher than 0");
            }
            if (!DateTime.TryParse(inventory.DateArrived, out DateTime parsedDate))
            {
                throw new BadRequest("Invalid Date");
            }
        }
    }
}
