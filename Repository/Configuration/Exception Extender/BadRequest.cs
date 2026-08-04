namespace ERP.Repository.Configuration.Exception_Extender
{
    public class BadRequest(string? message = null, Exception? innerException = null) : Exception(message, innerException)
    {
    }
}
