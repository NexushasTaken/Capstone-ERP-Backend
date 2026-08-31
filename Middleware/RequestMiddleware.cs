using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;

namespace ERP.Middleware
{
    public class RequestMiddleware(RequestDelegate _next, ResponseHelper _response)
    {
        //private readonly RequestDelegate _next = next;
        //private readonly ResponseHelper _response = responseHelper;


        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BadRequest ex)
            {
                await Response(context, 400, "application/json", ex.Message);
            }
            catch(UnauthorizedAccessException ex)
            {
                await Response(context, 401, "application/json", ex.Message);
            }
            catch(NotFound ex)
            {
                await Response(context, 404, "application/json", ex.Message);
            }
            catch (Exception ex) { 
                await Response(context, 500, "application/json", ex.Message);
            }
        }

        public async Task<HttpContext> Response(HttpContext context, int statusCode, string contentType, string error)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = contentType;
            await context.Response.WriteAsJsonAsync(_response.Status(statusCode,false,error,null));

            return context;
        }
    }
}
