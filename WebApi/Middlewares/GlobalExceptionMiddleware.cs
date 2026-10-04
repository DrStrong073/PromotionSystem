using Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace WebApi.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var response = new { Message = "Đã có lỗi hệ thống xảy ra.", Detail = exception.Message };

            if (exception is BadRequestException badRequestEx)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new { Message = badRequestEx.Message, Detail = "" };
            }

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
