using FluentValidation;

namespace ERP.Repository.Configuration.Validation
{
    public record PageQuery(int Page, int PageSize);

    public class PageQueryValidator : AbstractValidator<PageQuery>
    {
        private static readonly PageQueryValidator Instance = new();

        public PageQueryValidator()
        {
            RuleFor(p => p.Page).GreaterThan(0).WithMessage("Page is required");
            RuleFor(p => p.PageSize).GreaterThan(0).WithMessage("Page Size is required");
        }

        // Paging comes from query parameters, not a body model, so services call this directly.
        public static void Ensure(int page, int pageSize) => Instance.EnsureValid(new PageQuery(page, pageSize));
    }
}
