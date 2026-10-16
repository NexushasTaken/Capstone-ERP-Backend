using ERP.Repository.ViewModel.Products;
using FluentValidation;

namespace ERP.Repository.Configuration.Validation.Products
{
    public class CategoryUpdateValidator : AbstractValidator<CategoryUpdateViewModel>
    {
        public CategoryUpdateValidator()
        {
            RuleFor(c => c.Id).GreaterThan(0).WithMessage("Category is required");
            RuleFor(c => c.Type).NotEmpty().WithMessage("Category Type is required");
        }
    }

    public class ProductPostValidator : AbstractValidator<ProductPostViewModel>
    {
        public ProductPostValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Product Name is required");
            RuleFor(p => p.Price).GreaterThan(0).WithMessage("Product price must be greater than 0");
        }
    }

    public class ProductUpdateValidator : AbstractValidator<ProductUpdateViewModel>
    {
        public ProductUpdateValidator()
        {
            Include(new ProductPostValidator());
            RuleFor(p => p.Id).GreaterThan(0).WithMessage("Product is required");
        }
    }

    public class CategoryImportValidator : AbstractValidator<CategoryImportViewModel>
    {
        public CategoryImportValidator()
        {
            RuleFor(c => c.Rows).NotEmpty().WithMessage(ImportLimits.EmptyMessage);
            RuleFor(c => c.Rows.Count)
                .LessThanOrEqualTo(ImportLimits.MaxRows)
                .WithMessage(ImportLimits.TooManyMessage)
                .OverridePropertyName("Rows");
            RuleForEach(c => c.Rows)
                .ChildRules(row => row.RuleFor(r => r.Type).NotEmpty().WithMessage("Category Name is required"));
        }
    }

    public class ProductImportRowValidator : AbstractValidator<ProductImportRow>
    {
        public ProductImportRowValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("Product Name is required");
            RuleFor(p => p.Price).GreaterThan(0).WithMessage("Product price must be greater than 0");

            // Stock columns are optional, but a stock record needs both.
            When(
                p => p.Quantity != null || p.ReorderPoint != null,
                () =>
                {
                    RuleFor(p => p.Quantity)
                        .NotNull()
                        .WithMessage("Quantity is required when a reorder point is given")
                        .GreaterThan(0)
                        .WithMessage("Quantity should be higher than 0");
                    RuleFor(p => p.ReorderPoint)
                        .NotNull()
                        .WithMessage("Reorder Point is required when a quantity is given")
                        .GreaterThan(0)
                        .WithMessage("Reorder Point should be higher than 0");
                }
            );
        }
    }

    public class ProductImportValidator : AbstractValidator<ProductImportViewModel>
    {
        public ProductImportValidator()
        {
            RuleFor(p => p.Rows).NotEmpty().WithMessage(ImportLimits.EmptyMessage);
            RuleFor(p => p.Rows.Count)
                .LessThanOrEqualTo(ImportLimits.MaxRows)
                .WithMessage(ImportLimits.TooManyMessage)
                .OverridePropertyName("Rows");
            RuleForEach(p => p.Rows).SetValidator(new ProductImportRowValidator());
        }
    }
}
