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
}
