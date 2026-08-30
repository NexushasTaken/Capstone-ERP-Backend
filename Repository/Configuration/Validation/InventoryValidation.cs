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

        public static void ValidateUpdate(InventoryUpdateViewModel inventory)
        {
            if (string.IsNullOrWhiteSpace(inventory.Name))
            {
                throw new BadRequest("Inventory Item name is required");
            }
            if (inventory.ProductId <= 0)
            {
                throw new BadRequest("Product is required");
            }
            if (inventory.WarehouseId <= 0)
            {
                throw new BadRequest("Warehouse is required");
            }
            if(inventory.Id <= 0)
            {
                throw new BadRequest("Inventory Item is required");
            }
        }

        public static void MarkAsDamagedValidation(InventoryDamagePostViewModel damaged)
        {
            if (damaged.Id <= 0)
            {
                throw new BadRequest("Inventory item is required");
            }
            if (damaged.Quantity <= 0)
            {
                throw new BadRequest("Please add quantity to mark as damage");
            }
            if (string.IsNullOrWhiteSpace(damaged.Reason))
            {
                throw new BadRequest("Please add a reason why you want to mark it as damage");
            }
        }

        public static void InventoryTransactionValidation(InventoryTransactionPostViewModel transaction)
        {
            if (transaction.Id <= 0)
            {
                throw new BadRequest("Inventory item is required");
            }
            if (transaction.Quantity == 0)
            {
                throw new BadRequest("Please add quantity to begin a transaction");
            }
            if (transaction.Label <= 0)
            {
                throw new BadRequest("Transaction label is required");
            }
        }

        public static void InventoryWareHouseValidation(InventoryWareHousePostViewModel wareHouse)
        {
            if (string.IsNullOrWhiteSpace(wareHouse.Name))
            {
                throw new BadRequest("Warehouse Name is required");
            }
            if (string.IsNullOrWhiteSpace(wareHouse.Address))
            {
                throw new BadRequest("Warehouse Address is required");
            }
            if (wareHouse.Capicity <= 0)
            {
                throw new BadRequest("Warehouse Capacity is required");
            }
        }

        public static void InventoryWareHouseUpdateValidation(InventoryWareHouseUpdateViewModel wareHouse)
        {
            var post = new InventoryWareHousePostViewModel
            {
                Name = wareHouse.Name,
                Address = wareHouse.Address,
                Capicity = wareHouse.Capicity
            };

            InventoryWareHouseValidation(post);

            if (wareHouse.Id <= 0)
            {
                throw new BadRequest("Warehouse is required");
            }
        }
    }
}
