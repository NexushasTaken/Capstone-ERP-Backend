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

    public class OrderPostValidator : AbstractValidator<OrderPostViewModel>
    {
        public OrderPostValidator()
        {
            RuleFor(o => o.OrderTypeId).GreaterThan(0).WithMessage("Order Type is required");
            RuleFor(o => o.DeliveryRiderId).GreaterThanOrEqualTo(0).WithMessage("Driver is Invalid");
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
}
