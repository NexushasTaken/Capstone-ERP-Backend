using ERP.Repository.ViewModel.Inventories;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Inventories
{
    public class InventoryPostValidator : AbstractValidator<InventoryPostViewModel>
    {
        public InventoryPostValidator()
        {
            RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity should be higher than 0");
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

    public class InventoryRestockValidator : AbstractValidator<InventoryRestockViewModel>
    {
        public InventoryRestockValidator()
        {
            RuleFor(r => r.Id).GreaterThan(0).WithMessage("Inventory item is required");
            RuleFor(r => r.Quantity).GreaterThan(0).WithMessage("Quantity should be higher than 0");
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
}
