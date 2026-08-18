using System.Text.Json;
using WealthMap.Application.Common.Exceptions;
using WealthMap.Domain.Exceptions;

namespace WealthMap.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteResponse(context, StatusCodes.Status400BadRequest, new
            {
                title = "Validation failed",
                status = 400,
                errors = ex.Errors
            });
        }
        catch (UnauthorizedException ex)
        {
            await WriteResponse(context, StatusCodes.Status401Unauthorized, new
            {
                title = "Unauthorized",
                status = 401,
                detail = ex.Message
            });
        }
        catch (DomainException ex)
        {
            await WriteResponse(context, StatusCodes.Status400BadRequest, new
            {
                title = "Business rule violation",
                status = 400,
                detail = ex.Message
            });
        }
        catch (NotFoundException ex)
{
    await WriteResponse(context, StatusCodes.Status404NotFound, new
    {
        title = "Resource not found",
        status = 404,
        detail = ex.Message
    });
}
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteResponse(context, StatusCodes.Status500InternalServerError, new
            {
                title = "An unexpected error occurred",
                status = 500
            });
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, object body)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}