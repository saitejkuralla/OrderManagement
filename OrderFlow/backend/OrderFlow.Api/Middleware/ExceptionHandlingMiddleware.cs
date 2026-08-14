using System.Net;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Exceptions;

namespace OrderFlow.Api.Middleware;

/// <summary>Translates Application-layer exceptions into consistent ProblemDetails HTTP responses.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.NotFound, "Not Found", ex.Message);
        }
        catch (BusinessRuleViolationException ex)
        {
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Business Rule Violation", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request");
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
