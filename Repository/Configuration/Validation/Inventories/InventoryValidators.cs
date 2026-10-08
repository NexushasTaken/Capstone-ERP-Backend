using ERP.Repository.ViewModel.Inventories;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Inventories
{
    public class InventoryPostValidator : AbstractValidator<InventoryPostViewModel>
    {
        public InventoryPostValidator()
        {
            RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity should be higher than 0");
            RuleFor(i => i.WarehouseId).GreaterThan(0).WithMessage("Invalid Warehouse ID");
            RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Invalid Product ID");
            RuleFor(i => i.ReorderPoint).GreaterThan(0).WithMessage("Reorder Point should be higher than 0");
        }
    }

    public class InventoryUpdateValidator : AbstractValidator<InventoryUpdateViewModel>
    {
        public InventoryUpdateValidator()
        {
            RuleFor(i => i.Id).GreaterThan(0).WithMessage("Inventory Item is required");
            RuleFor(i => i.ReorderPoint).GreaterThan(0).WithMessage("Reorder Point should be higher than 0");
        }
    }

    public class InventoryDamageValidator : AbstractValidator<InventoryDamagePostViewModel>
    {
        public InventoryDamageValidator()
        {
            RuleFor(d => d.Id).GreaterThan(0).WithMessage("Inventory item is required");
            RuleFor(d => d.Quantity).GreaterThan(0).WithMessage("Please add quantity to mark as damage");
            RuleFor(d => d.Reason).NotEmpty().WithMessage("Please add a reason why you want to mark it as damage");
        }
    }

    public class InventoryTransactionValidator : AbstractValidator<InventoryTransactionPostViewModel>
    {
        public InventoryTransactionValidator()
        {
            RuleFor(t => t.Id).GreaterThan(0).WithMessage("Inventory item is required");
            RuleFor(t => t.Quantity).NotEqual(0).WithMessage("Please add quantity to begin a transaction");
            RuleFor(t => t.Label).GreaterThan(0).WithMessage("Transaction label is required");
        }
    }

    public class WarehousePostValidator : AbstractValidator<InventoryWareHousePostViewModel>
    {
        public WarehousePostValidator()
        {
            RuleFor(w => w.Name).NotEmpty().WithMessage("Warehouse Name is required");
            RuleFor(w => w.Address).NotEmpty().WithMessage("Warehouse Address is required");
        }
    }

    public class WarehouseUpdateValidator : AbstractValidator<InventoryWareHouseUpdateViewModel>
    {
        public WarehouseUpdateValidator()
        {
            Include(new WarehousePostValidator());
            RuleFor(w => w.Id).GreaterThan(0).WithMessage("Warehouse is required");
        }
    }
}
