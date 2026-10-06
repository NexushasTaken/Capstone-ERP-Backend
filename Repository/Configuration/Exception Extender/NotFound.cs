namespace ERP.Repository.Configuration.Exception_Extender
{
    public class NotFound(string? message = null, Exception? innerException = null)
        : Exception(message, innerException) { }
}
