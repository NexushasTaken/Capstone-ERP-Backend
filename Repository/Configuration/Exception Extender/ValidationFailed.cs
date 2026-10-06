namespace ERP.Repository.Configuration.Exception_Extender
{
    // A 400 that names every rejected field. Keys are camelCase paths matching the JSON payload,
    // e.g. "name" or "orderLines.0.quantity".
    public class ValidationFailed(IDictionary<string, string[]> errors)
        : Exception(errors.Values.SelectMany(e => e).FirstOrDefault() ?? "Validation failed")
    {
        public IDictionary<string, string[]> Errors { get; } = errors;

        public ValidationFailed(string field, string message)
            : this(new Dictionary<string, string[]> { [field] = [message] }) { }
    }
}
