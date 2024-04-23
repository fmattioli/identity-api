using Contracts.Web.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace SpendManagement.Identity.IoC.Extensions
{
    public class GlobalExceptionHandlerExtensions(ILogger logger) : IExceptionHandler
    {
        private readonly ILogger _logger = logger;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var code = exception switch
            {
                NotFoundException => HttpStatusCode.NotFound,
                InternalServerErrorException => HttpStatusCode.InternalServerError,
                ValidationException => HttpStatusCode.BadRequest,
                BadRequestException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                HttpRequestException => HttpStatusCode.InternalServerError,
                _ => HttpStatusCode.InternalServerError,
            };

            var errors = new List<string>();
            foreach (var key in exception.Data.Keys)
            {
                _logger.Error("The following error occured: {ErrorMessage}", exception.Data[key]);
                _logger.Error(exception, "More details can be find following the exception");
                errors.Add(exception.Data[key]?.ToString() ?? "");
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)code,
                Title = string.Join(",", errors)
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
