using System.Text.RegularExpressions;
using ERP.Repository.Configuration.Exception_Extender;
using FluentValidation;
using FluentValidation.Results;

namespace ERP.Repository.Configuration.Validation
{
    public static partial class ValidatorExtensions
    {
        // Runs the validator and throws ValidationFailed with every failure, keyed by camelCase path.
        public static async Task EnsureValidAsync<T>(
            this IValidator<T> validator,
            T model,
            CancellationToken cancellation = default
        )
        {
            var result = await validator.ValidateAsync(model, cancellation);
            ThrowIfInvalid(result);
        }

        public static void EnsureValid<T>(this IValidator<T> validator, T model)
        {
            ThrowIfInvalid(validator.Validate(model));
        }

        private static void ThrowIfInvalid(ValidationResult result)
        {
            if (result.IsValid)
                return;

            var errors = result
                .Errors.GroupBy(e => ToFieldPath(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

            throw new ValidationFailed(errors);
        }

        // "OrderLines[0].Quantity" → "orderLines.0.quantity", the path format react-hook-form uses.
        // Also accepts ASP.NET model-binding keys such as "$.orderLines[0].quantity".
        public static string ToFieldPath(string propertyName)
        {
            var dotted = IndexPattern().Replace(propertyName.TrimStart('$', '.'), ".$1");
            return string.Join(
                '.',
                dotted
                    .Split('.', StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => char.ToLowerInvariant(part[0]) + part[1..])
            );
        }

        [GeneratedRegex(@"\[(\d+)\]")]
        private static partial Regex IndexPattern();
    }
}
