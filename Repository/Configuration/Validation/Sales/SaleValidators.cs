using ERP.Repository.Configuration.Enum;
using ERP.Repository.ViewModel.Sales;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Sales
{
    public class SaleReturnItemValidator : AbstractValidator<SaleReturnItemViewModel>
    {
        public SaleReturnItemValidator()
        {
            RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Product is required");
            RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity should be higher than 0");
            RuleFor(i => i.ConditionId)
                .Must(c => System.Enum.IsDefined(typeof(ReturnConditionEnum), c))
                .WithMessage("Condition must be good or damaged");
        }
    }

    public class SaleReturnValidator : AbstractValidator<SaleReturnPostViewModel>
    {
        public SaleReturnValidator()
        {
            RuleFor(r => r.SaleId).GreaterThan(0).WithMessage("Sale is required");
            RuleFor(r => r.Reason).NotEmpty().WithMessage("Please add a reason for the return");
            RuleFor(r => r.Items).NotEmpty().WithMessage("Add at least one product to return");
            RuleFor(r => r.Items)
                .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
                .WithMessage("Each product can only be listed once");
            RuleForEach(r => r.Items).SetValidator(new SaleReturnItemValidator());
        }
    }
}
