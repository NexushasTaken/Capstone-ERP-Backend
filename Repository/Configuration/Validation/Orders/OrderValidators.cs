using ERP.Repository.Configuration.Helper;
using ERP.Repository.ViewModel.Orders;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Orders
{
    public class OrderLineValidator : AbstractValidator<OrderLineViewModel>
    {
        public OrderLineValidator()
        {
            RuleFor(l => l.ProductId).GreaterThan(0).WithMessage("Product is required");
            RuleFor(l => l.Quantity).GreaterThan(0).WithMessage("Quantity is required");
        }
    }

    // Shared by create and edit. Who may pick a custom discount is checked in OrderService.
    public class OrderDiscountValidator : AbstractValidator<IOrderDiscount>
    {
        public OrderDiscountValidator()
        {
            RuleFor(o => o.DiscountType)
                .Must(t => DiscountTypes.All.Contains(t))
                .WithMessage("Discount type must be none, senior, pwd, solo_parent or custom");

            When(
                o => o.DiscountType == DiscountTypes.None,
                () => RuleFor(o => o.DiscountPercent).Equal(0).WithMessage("Discount must be 0 when there is none")
            );

            When(
                o => DiscountTypes.IsPreset(o.DiscountType),
                () =>
                    RuleFor(o => o.DiscountPercent)
                        .Must((o, percent) => percent == DiscountTypes.PresetRates[o.DiscountType])
                        .WithMessage(o =>
                            $"The {DiscountTypes.Label(o.DiscountType)} discount is fixed at {DiscountTypes.PresetRates[o.DiscountType]}%"
                        )
            );

            When(
                o => o.DiscountType == DiscountTypes.Custom,
                () => RuleFor(o => o.DiscountPercent).GreaterThan(0).WithMessage("Custom discount must be more than 0")
            );
        }
    }

    public class OrderPostValidator : AbstractValidator<OrderPostViewModel>
    {
        public OrderPostValidator()
        {
            RuleFor(o => o.OrderTypeId).GreaterThan(0).WithMessage("Order Type is required");
            RuleFor(o => o.DeliveryDriverId).GreaterThanOrEqualTo(0).WithMessage("Driver is Invalid");
            RuleFor(o => o.DiscountPercent).InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100");
            Include(new OrderDiscountValidator());
            RuleFor(o => o.OrderLines).NotEmpty().WithMessage("Add at least one product");
            RuleForEach(o => o.OrderLines).SetValidator(new OrderLineValidator());
        }
    }

    public class OrderPutValidator : AbstractValidator<OrderPutViewModel>
    {
        public OrderPutValidator()
        {
            RuleFor(o => o.OrderId).GreaterThan(0).WithMessage("Order is required");
            RuleFor(o => o.DeliveryDriverId).GreaterThanOrEqualTo(0).WithMessage("Driver is Invalid");
            RuleFor(o => o.DiscountPercent).InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100");
            Include(new OrderDiscountValidator());
            RuleFor(o => o.OrderLines).NotEmpty().WithMessage("Add at least one product");
            RuleForEach(o => o.OrderLines).SetValidator(new OrderLineValidator());
        }
    }

    public class OrderStatusValidator : AbstractValidator<OrderStatusPostViewModel>
    {
        public OrderStatusValidator()
        {
            RuleFor(s => s.OrderId).GreaterThan(0).WithMessage("Order is required");
            RuleFor(s => s.OrderStatusId).GreaterThan(0).WithMessage("Order Status is required");
        }
    }

    public class DriverPostValidator : AbstractValidator<DeliveryDriverPostViewModel>
    {
        public DriverPostValidator()
        {
            RuleFor(d => d.FirstName).NotEmpty().WithMessage("First Name is required");
        }
    }

    public class DriverPatchValidator : AbstractValidator<DeliveryDriverPatchViewModel>
    {
        public DriverPatchValidator()
        {
            Include(new DriverPostValidator());
        }
    }

    public class DriverImportValidator : AbstractValidator<DriverImportViewModel>
    {
        public DriverImportValidator()
        {
            RuleFor(d => d.Rows).NotEmpty().WithMessage(ImportLimits.EmptyMessage);
            RuleFor(d => d.Rows.Count)
                .LessThanOrEqualTo(ImportLimits.MaxRows)
                .WithMessage(ImportLimits.TooManyMessage)
                .OverridePropertyName("Rows");
            RuleForEach(d => d.Rows).SetValidator(new DriverPostValidator());
        }
    }
}
