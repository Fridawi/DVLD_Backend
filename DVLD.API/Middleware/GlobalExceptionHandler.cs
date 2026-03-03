using DVLD.CORE.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace DVLD.API.Middleware
{
    public class GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly IHostEnvironment _env = env;
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;

            _logger.LogError(exception, "An error occurred: {Message} | Path: {Path} | TraceId: {TraceId}",
                exception.Message, httpContext.Request.Path, traceId);

            var (statusCode, title) = exception switch
            {
                ResourceNotFoundException or KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found"),
                ValidationException or ArgumentException          => ((int)HttpStatusCode.BadRequest, "Bad Request"),
                ConflictException                                 => ((int)HttpStatusCode.Conflict, "Conflict Detected"),
                ForbiddenException                                => ((int)HttpStatusCode.Forbidden, "Access Denied"),
                UnauthorizedAccessException                       => ((int)HttpStatusCode.Unauthorized, "Unauthorized Access"),
                _                                          => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };


            problemDetails.Extensions["traceId"] = traceId;

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
