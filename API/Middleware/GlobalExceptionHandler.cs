using Microsoft.AspNetCore.Diagnostics;
using API.Exceptions;
using Shared.DTOs;
using Application.Exceptions;

namespace API.Middleware
{
    /// <summary>Converts unhandled exceptions to consistent API error responses.</summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

            var response = httpContext.Response;
            response.ContentType = "application/json";

            var apiErrorResponse = exception switch
            {
                EntityNotFoundException ex => new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    response = ApiErrorResponse.Create(ex.Message, traceId: traceId)
                },
                InvalidOperationApiException ex => new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    response = ApiErrorResponse.Create(ex.Message, traceId: traceId)
                },
                ApplicationValidationException ex => new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    response = ApiErrorResponse.Create(ex.Message, ex.Errors, traceId)
                },
                ConflictException ex => new
                {
                    statusCode = StatusCodes.Status409Conflict,
                    response = ApiErrorResponse.Create(ex.Message, traceId: traceId)
                },
                _ => new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    response = ApiErrorResponse.Create("An unexpected error occurred. Please try again later.", traceId: traceId)
                }
            };

            response.StatusCode = apiErrorResponse.statusCode;
            await response.WriteAsJsonAsync(apiErrorResponse.response, cancellationToken);

            return true;
        }
    }

}
