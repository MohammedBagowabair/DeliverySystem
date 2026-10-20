using System.Text.Json;
using Application.Common.Models;
using Domain.Exceptions;

namespace WebApi.Middleware
{
    /// <summary>
    /// Catches unhandled exceptions and returns a safe Arabic ApiResultModel envelope.
    /// Full exception details are logged server-side only.
    /// </summary>
    public sealed class GlobalExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                await WriteErrorAsync(context, ex);
            }
        }

        private async Task WriteErrorAsync(HttpContext context, Exception ex)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogError(ex, "Unhandled exception after response started");
                throw ex;
            }

            _logger.LogError(ex, "Unhandled exception");

            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = StatusCodes.Status200OK;

            ApiResultModel<object> result;
            if (ex is DeliveryCoreException domain)
            {
                result = new ApiResultModel<object>(domain.Code, domain.Message, null!)
                {
                    Error = domain.Data
                };
            }
            else
            {
                result = new ApiResultModel<object>(
                    ErrorCodes.SYS_UNKNOWN_ERROR_CODE,
                    ErrorCodes.SYS_UNKNOWN_ERROR_MESSAGE,
                    null!);
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(result, JsonOptions));
        }
    }
}
