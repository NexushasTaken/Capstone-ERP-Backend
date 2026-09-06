using ERP.Controllers.InventoryController;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.InventoryData;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Model.Inventories;
using ERP.Repository.ViewModel.Inventories;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.ConstrainedExecution;

namespace ERP.Repository.Services.Inventories
{
    public class InventoryService(IInventoryData _inventory) : IInventoryService
    {

        #region Inventory
        public async Task<InventoryPageViewModel> GetInventories(int page, int pageSize, string name, int filter,int statusId, int wareHousePresent, CancellationToken cancellationToken)
        {

            var inv = await _inventory.GetInventoriesWithoutTracking(page, pageSize, name.ToLower(), filter, statusId, wareHousePresent, cancellationToken);

            var totalCount = await _inventory.InventoryCount(name.ToLower(), filter, statusId, wareHousePresent);

            var inventory = inv.Select(i => new InventoryViewModel
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = i.Name,
                Quantity = i.Quantity,
                ReorderPoint = i.ReorderPoint,
                WarehouseId = i.WarehouseId ?? 0,
                WarehouseName = i.Warehouse?.Name ?? "No Warehouse",
                Status = i.InventoryStatus.Status,
                DateArrived = i.DateArrived,
                Created_At = i.Created_At
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
            var wareHouseCapacity = await _inventory.GetIndividualWarehousesMaxCapacityWithoutTracking(inventory.WarehouseId);

            if(inventory.Quantity > wareHouseCapacity)
            {
                throw new BadRequest("The item quantity exceeds the warehouse capacity.");
            }

            if (id > 0)
            {
                var existing = await _inventory.GetInventoryWithTracking(id);

                existing.Quantity += inventory.Quantity;
                existing.Updated_At = DateTime.UtcNow;

                existing.StatusId = ReorderRatio(existing.Quantity, existing.ReorderPoint);

                await _inventory.SaveChanges();

                var transaction = new InventoryTransactionPostViewModel
                {
                    Id = existing.Id,
                    Quantity =+ inventory.Quantity,
                    Label = inventory.InventoryLabelId
                };

                await InventoryTransaction(transaction);

                return;
            }

            InventoryValidation.ValidateItem(inventory);

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
            inventory.Deleted_At = DateTime.UtcNow;

            await _inventory.SaveChanges();
        }

        public async Task UpdateInventory(InventoryUpdateViewModel inventory)
        {
            InventoryValidation.ValidateUpdate(inventory);

            var existing = await _inventory.GetInventoryWithTracking(inventory.Id);

            if (existing == null)
            {
                throw new NotFound("Inventory Item Not Found");
            }

            existing.Name = inventory.Name.ToLower()    ;
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

        public async Task MarkAsDamaged(InventoryDamagePostViewModel damaged)
        {
            InventoryValidation.MarkAsDamagedValidation(damaged);

            var inventory = await _inventory.GetInventoryWithTracking(damaged.Id);

            if (inventory == null)
            {
                throw new NotFound("Inventory item not found");
            }

            if(inventory.Quantity < damaged.Quantity)
            {
                throw new BadRequest("Quantity is greater than what you have in stocks");
            }

            var sub = inventory.Quantity - damaged.Quantity;

            inventory.Quantity = sub;
            inventory.Updated_At = DateTime.UtcNow;
            inventory.StatusId = ReorderRatio(sub, inventory.ReorderPoint);


            await _inventory.SaveChanges();

            var damn = new DamagedInventory
            {
                InventoryId = inventory.Id,
                Reason = damaged.Reason,
                Quantity = damaged.Quantity,
                Created_At = DateTime.UtcNow,
                IsActive = true
            };

            await _inventory.Save(damn);

            var transaction = new InventoryTransactionPostViewModel
            {
                Id = inventory.Id,
                Quantity = -damaged.Quantity,
                Label = 2
            };

            await InventoryTransaction(transaction);

            return;
        }

        public async Task InventoryTransaction(InventoryTransactionPostViewModel transaction)
        {
            InventoryValidation.InventoryTransactionValidation(transaction);

            var transac = new InventoryTransaction
            {
                InventoryId = transaction.Id,
                QuantityChanged = transaction.Quantity,
                InventoryLabelId = transaction.Label,
                Created_At = DateTime.UtcNow,
                IsActive = true 
            };

            await _inventory.Save(transac);

            return;
        }

        #endregion

        #region Warehouse
        public async Task<IEnumerable<InventoryWareHouseViewModel>> GetWarehouses()
        {
            var wareHouses = await _inventory.GetWarehousesWithoutTracking();

            if(wareHouses == null)
            {
                throw new NotFound("No warehouse is detected. please add first");
            }

            var result = new List<InventoryWareHouseViewModel>();
            var totalCapacity = 0;
            var totalStocks = 0;


            foreach (var wh in wareHouses)
            {
                var tasks = await _inventory.GetIndividualWarehouseCurrentCapacityWithoutTracking(wh.Id);

                result.Add(new InventoryWareHouseViewModel
                {
                    Id = wh.Id,
                    Name = wh.Name,
                    Address = wh.Address,
                    Capicity = wh.Capacity,
                    Stocks = tasks
                });

                totalCapacity += wh.Capacity;
                totalStocks += tasks;
            }

            result.Add(new InventoryWareHouseViewModel
            {
                Id = result.Count() + 1,
                Name = "All Warehouse Record",
                Address = "",
                Capicity = totalCapacity,
                Stocks = totalStocks
            });

            return result;
        }

        public async Task NewWareHouse(InventoryWareHousePostViewModel wareHouse)
        {
            InventoryValidation.InventoryWareHouseValidation(wareHouse);

            var wh = new Warehouse
            {
                Name = wareHouse.Name.ToLower(),
                Address = wareHouse.Address.ToLower(),
                Capacity = wareHouse.Capicity,
                Created_At = DateTime.UtcNow,
                IsActive = true
            };

            await _inventory.Save(wh);

            return;
        }

        public async Task UpdateWareHouse(InventoryWareHouseUpdateViewModel wareHouse)
        {
            InventoryValidation.InventoryWareHouseUpdateValidation(wareHouse);

            var wh = await _inventory.GetIndividualWareHouseWithTracking(wareHouse.Id);

            if(wh == null)
            {
                throw new NotFound("Warehouse not found");
            }

            wh.Name = wareHouse.Name.ToLower();
            wh.Address = wareHouse.Address.ToLower();
            wh.Capacity = wareHouse.Capicity;
            wh.Updated_At = DateTime.UtcNow;

            await _inventory.SaveChanges();

            return;
        }

        public async Task DeleteWareHouse(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Warehouse is required");
            }

            var wareHouse = await _inventory.GetIndividualWareHouseWithTracking(id);

            if (wareHouse == null)
            {
                throw new NotFound("Warehouse not found");
            }

            wareHouse.IsActive = false;
            wareHouse.Deleted_At = DateTime.UtcNow;

            var inventories = await _inventory.GetInventoryWithWareHouseId(id);

            foreach (var inv in inventories)
            {
                inv.WarehouseId = null;
                inv.Updated_At = DateTime.UtcNow;
            }

            await _inventory.SaveChanges();

            return;
        }
        #endregion

        #region InventoryLabel
        public async Task<IEnumerable<InventoryLabelViewModel>> GetInventoryLabels()
        {
            var label = await _inventory.GetLabelForInsert();

            var final = label.Select(i => new InventoryLabelViewModel
            {
                Id = i.Id,
                Type = i.Type
            });

            return final;
        }
        #endregion

        #region Damage Inventory
        public async Task<IEnumerable<InventoryDamageViewModel>> GetDamageInventory(int id)
        {
            if(id <= 0)
            {
                throw new BadRequest("Inventory item is required");
            }

            var damaged = await _inventory.GetDamageInventoryWithoutTracking(id);

            var final = damaged.Select(i => new InventoryDamageViewModel
            {
                Reason = i.Reason,
                Quantity = i.Quantity,
                Created_At = i.Created_At,
            });

            return final;
        }
        #endregion

        #region Trasactions
        public async Task<IEnumerable<InventoryTransactionViewModle>> GetInventoryItemTransaction(int id)
        {
            var transac = await _inventory.GetItemTransactionWithoutTracking(id);

            var final = transac.Select(t => new InventoryTransactionViewModle
            {
                Quantity = t.QuantityChanged,
                Label = t.InventoryLabel.Type,
                Created_At = t.Created_At

            });

            return final;
        }
        #endregion

        #region Movements Velocity

        public async Task<InventoryMovementVelocityPageViewModel> GetMovementVelocity(int cutOffDate, int page, int pageSize)
        {
            var inventory = await _inventory.GetMovementVelocityWithoutTracking(cutOffDate, page, pageSize);

            var total = await _inventory.GetMovementVelocityCount(cutOffDate);


            var result = new InventoryMovementVelocityPageViewModel
            {
                inventories = inventory.Select(i => new InventoryMovementVelocityViewModel
                {
                    InventoryId = i.InventoryId,
                    Name = i.Name,
                    Classification = i.Classification,
                    VelocityMetric = i.VelocityMetric,
                    Warehouse = i.Warehouse
                }),
                PageCount = (int)Math.Ceiling(total / (double)pageSize),
                Rows = total
            };

            return result;
        }
        #endregion
    }
}