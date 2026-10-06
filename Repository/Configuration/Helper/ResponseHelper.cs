using ERP.Repository.ViewModel;

namespace ERP.Repository.Configuration.Helper
{
    public class ResponseHelper
    {
        public object Status(
            int status,
            bool success,
            string? message,
            object? content = null,
            IDictionary<string, string[]>? errors = null
        )
        {
            return new ResponseHelperViewModel
            {
                Status = status,
                Success = success,
                Message = message,
                Content = content,
                Errors = errors,
            };
        }
    }
}
