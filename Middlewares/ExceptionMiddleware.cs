using System.Net;
using System.Text.Json;
using HotelManagement.Exceptions;

namespace HotelManagement.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Exception occurred after the response started for {Path}", context.Request.Path);
                    throw;
                }
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                BadRequestException => (int)HttpStatusCode.BadRequest,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            if (statusCode >= (int)HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
            }
            else
            {
                _logger.LogWarning("Request {Path} failed with {StatusCode}: {Message}",
                    context.Request.Path, statusCode, exception.Message);
            }

            var response = new
            {
                statusCode,
                success = false,
                message = statusCode == (int)HttpStatusCode.InternalServerError
                    ? "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau!"
                    : exception.Message,
                errors = (object?)null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
