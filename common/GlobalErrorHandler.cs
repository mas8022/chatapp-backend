using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using FluentValidationException = FluentValidation.ValidationException;

namespace backend.common
{
    public class GlobalExceptionHandler(IHostEnvironment env) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (status, message, data) = exception switch
            {
                FluentValidationException ex => (
                    StatusCodes.Status422UnprocessableEntity,
                    "Validation failed.",
                    (object?)ex.Errors.Select(error => new
                    {
                        Field = error.PropertyName,
                        Message = error.ErrorMessage
                    })
                ),

                BadRequestException ex => (
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    null
                ),

                UnauthorizedException ex => (
                    StatusCodes.Status401Unauthorized,
                    ex.Message,
                    null
                ),

                ForbiddenException ex => (
                    StatusCodes.Status403Forbidden,
                    ex.Message,
                    null
                ),

                NotFoundException ex => (
                    StatusCodes.Status404NotFound,
                    ex.Message,
                    null
                ),

                ConflictException ex => (
                    StatusCodes.Status409Conflict,
                    ex.Message,
                    null
                ),

                TooManyRequestsException ex => (
                    StatusCodes.Status429TooManyRequests,
                    ex.Message,
                    null
                ),

                InternalServerException ex => (
                    StatusCodes.Status500InternalServerError,
                    ex.Message,
                    null
                ),

                ServiceUnavailableException ex => (
                    StatusCodes.Status503ServiceUnavailable,
                    ex.Message,
                    null
                ),

                UnauthorizedAccessException => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized access.",
                    null
                ),

                AppException ex => (
                    ex.StatusCode,
                    ex.Message,
                    null
                ),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    env.IsDevelopment()
                        ? new
                        {
                            ExceptionType = exception.GetType().Name,
                            Detail = exception.Message,
                            StackTrace = exception.StackTrace
                        }
                        : null
                )
            };

            var result = new Result
            {
                Status = status,
                Message = message,
                Data = data,
                TraceId = httpContext.TraceIdentifier
            };

            httpContext.Response.StatusCode = status;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(
                result,
                cancellationToken: cancellationToken
            );

            return true;
        }
    }

    public abstract class AppException : Exception
    {
        public int StatusCode { get; }

        protected AppException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, StatusCodes.Status400BadRequest)
        {
        }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "You are not authorized.")
            : base(message, StatusCodes.Status401Unauthorized)
        {
        }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You do not have permission to access this resource.")
            : base(message, StatusCodes.Status403Forbidden)
        {
        }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base(message, StatusCodes.Status404NotFound)
        {
        }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, StatusCodes.Status409Conflict)
        {
        }
    }

    public class ValidationException : AppException
    {
        public ValidationException(string message)
            : base(message, StatusCodes.Status422UnprocessableEntity)
        {
        }
    }

    public class TooManyRequestsException : AppException
    {
        public TooManyRequestsException(string message = "Too many requests. Please try again later.")
            : base(message, StatusCodes.Status429TooManyRequests)
        {
        }
    }

    public class InternalServerException : AppException
    {
        public InternalServerException(string message = "An internal server error occurred.")
            : base(message, StatusCodes.Status500InternalServerError)
        {
        }
    }

    public class ServiceUnavailableException : AppException
    {
        public ServiceUnavailableException(string message = "Service is temporarily unavailable.")
            : base(message, StatusCodes.Status503ServiceUnavailable)
        {
        }
    }


}
