using ERP.Repository.Configuration.Helper;
using ERP.Repository.ViewModel.Purchases;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Purchases
{
    public class PurchasePostValidator : AbstractValidator<PurchasePostViewModel>
    {
        public PurchasePostValidator()
        {
            RuleFor(p => p.SupplierName).MaximumLength(100).WithMessage("Supplier name can be at most 100 characters");
            RuleFor(p => p.Note).MaximumLength(500).WithMessage("Note can be at most 500 characters");
            RuleFor(p => p.OrderedOn)
                .NotEmpty()
                .WithMessage("Order date is required")
                .Must(d => d <= DemandWeek.PhToday())
                .WithMessage("Order date can't be in the future");
            RuleFor(p => p.Lines).NotEmpty().WithMessage("Add at least one product");
            RuleFor(p => p.Lines)
                .Must(lines => lines.Select(l => l.ProductId).Distinct().Count() == lines.Count)
                .WithMessage("Each product can only be on one line");
            RuleForEach(p => p.Lines)
                .ChildRules(line =>
                {
                    line.RuleFor(l => l.ProductId).GreaterThan(0).WithMessage("Select a product");
                    line.RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity should be higher than 0");
                    line.RuleFor(l => l.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit cost can't be negative");
                });
        }
    }

    public class PurchasePutValidator : AbstractValidator<PurchasePutViewModel>
    {
        public PurchasePutValidator()
        {
            Include(new PurchasePostValidator());
            RuleFor(p => p.Id).GreaterThan(0).WithMessage("Purchase is required");
        }
    }

    public class PurchaseReceiveValidator : AbstractValidator<PurchaseReceiveViewModel>
    {
        public PurchaseReceiveValidator()
        {
            RuleFor(p => p.Id).GreaterThan(0).WithMessage("Purchase is required");
            RuleFor(p => p.Lines).NotEmpty().WithMessage("Enter what arrived");
            RuleForEach(p => p.Lines)
                .ChildRules(line =>
                    line.RuleFor(l => l.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity can't be negative")
                );
            RuleFor(p => p.Lines)
                .Must(lines => lines.Sum(l => l.Quantity) > 0)
                .WithMessage("Nothing arrived; cancel the purchase instead");
        }
    }
}
