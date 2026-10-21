using ERP.Repository.Configuration.Enum;
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

    public class StockAdjustmentValidator : AbstractValidator<StockAdjustmentPostViewModel>
    {
        public StockAdjustmentValidator()
        {
            RuleFor(a => a.Id).GreaterThan(0).WithMessage("Inventory item is required");
            RuleFor(a => a.ReasonId)
                .Must(r => r >= (int)AdjustmentReasonEnum.Damaged && r <= (int)AdjustmentReasonEnum.CountCorrection)
                .WithMessage("Choose a reason: damaged, lost, found or count correction");
            RuleFor(a => a.Quantity)
                .GreaterThan(0)
                .When(a => a.ReasonId != (int)AdjustmentReasonEnum.CountCorrection)
                .WithMessage("Quantity should be higher than 0");
            RuleFor(a => a.Quantity)
                .GreaterThanOrEqualTo(0)
                .When(a => a.ReasonId == (int)AdjustmentReasonEnum.CountCorrection)
                .WithMessage("Counted quantity can't be negative");
            RuleFor(a => a.Note).MaximumLength(500).WithMessage("Note can be at most 500 characters");
        }
    }
}
