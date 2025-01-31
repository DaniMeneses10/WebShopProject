using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebShopAPI.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
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

            var response = exception switch
            {
                KeyNotFoundException => new { StatusCode = (int)HttpStatusCode.NotFound, Message = exception.Message },
                UnauthorizedAccessException => new { StatusCode = (int)HttpStatusCode.Unauthorized, Message = "You are not authorized to perform this action." },
                ArgumentException => new { StatusCode = (int)HttpStatusCode.BadRequest, Message = exception.Message },
                _ => new { StatusCode = (int)HttpStatusCode.InternalServerError, Message = "An unexpected error occurred." }
            };

            context.Response.StatusCode = response.StatusCode;
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
