using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Repository.Services.Inventories
{
    public class InventoryService(IInventoryData _inventory) : IInventoryService
    {
        public async Task<InventoryPageViewModel> GetInventories(int page, int pageSize, string searchString, int filter,int statusId, CancellationToken cancellationToken)
        {
            var inv = await _inventory.GetInventoriesWithoutTracking(page, pageSize, searchString.ToLower(), filter, statusId, cancellationToken);

            var totalCount = await _inventory.InventoryCount(searchString.ToLower(), filter, statusId);

            var inventory = inv.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = i.Name,
                Quantity = i.Quantity,
                ReorderPoint = i.ReorderPoint,
                WarehouseId = i.WarehouseId,
                WarehouseName = i.Warehouse.Name,
                Status = i.InventoryStatus.Status,
                DateArrived = i.DateArrived
            });

            var result = new InventoryPageViewModel
            {
                inventories = inventory,
                PageCount = (int)Math.Ceiling(totalCount / (double)pageSize),
                Rows = totalCount
            };

            return result;
        }

        public async Task InsertItem(InventoryPostViewModel inventory, int id)
        {
            InventoryValidation.ValidateItem(inventory);

            if(id > 0)
            {
                var existing = await _inventory.GetInventoryWithTracking(id);

                existing.Quantity = inventory.Quantity;
                existing.Updated_At = DateTime.UtcNow;

                existing.StatusId = ReorderRatio(existing.Quantity,existing.ReorderPoint);

                await _inventory.SaveChanges();

                return;
            }

            DateTime.TryParse(inventory.DateArrived, out DateTime parsedDate);

            parsedDate = parsedDate.ToUniversalTime();

            var inv = new Inventory
            {
                Name = inventory.Name.ToLower(),
                Quantity = inventory.Quantity,
                WarehouseId = inventory.WarehouseId,
                ProductId = inventory.ProductId,
                ReorderPoint = inventory.ReorderPoint,
                DateArrived = parsedDate,
                StatusId = ReorderRatio(inventory.Quantity, inventory.ReorderPoint),
                IsActive = true,
                Created_At = DateTime.UtcNow,
            };

            await _inventory.Save(inv);

            return;
        }

        public async Task DeleteItem(int id)
        {
            if(id <= 0)
            {
                throw new BadRequest("Id is required");
            }

            var inventory = await _inventory.GetInventoryWithTracking(id);

            inventory.IsActive = false;

            await _inventory.SaveChanges();
        }

        public async Task<int> StatusInventoryCount(int id)
        {
            if(id <= 0)
            {
                throw new BadRequest("Id must be greater than 0");
            }

            var count = await _inventory.GetStatusCountInventory(id);

            return count;
        }

        public async Task UpdateInventory(InventoryUpdateViewModel inventory)
        {
            InventoryValidation.ValidateUpdate(inventory);

            var existing = await _inventory.GetInventoryWithTracking(inventory.Id);

            if (existing == null)
            {
                throw new NotFound("Inventory Item Not Found");
            }

            existing.Name = inventory.Name;
            existing.ProductId = inventory.ProductId;
            existing.WarehouseId = inventory.WarehouseId;
            existing.ReorderPoint = inventory.ReorderPoint;

            existing.StatusId = ReorderRatio(existing.Quantity, existing.ReorderPoint);

            await _inventory.SaveChanges();

            return;
        }

        private int ReorderRatio(int quantity, int reorderPoint)
        {
            double ratio = (double)quantity / reorderPoint;

            if (ratio >= 2.0)
            {
                return 2;
            }
            else if (ratio >= 1.0)
            {
                return 3;
            }
            return 1;
        }
    }
}
