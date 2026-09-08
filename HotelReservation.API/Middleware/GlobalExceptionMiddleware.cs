using HotelReservation.API.Models;
using HotelReservation.Application.Exceptions;

namespace HotelReservation.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        int statusCode = exception switch
        {
            ValidationException =>
                StatusCodes.Status400BadRequest,

            UnauthorizedException =>
                StatusCodes.Status401Unauthorized,

            ForbiddenException =>
                StatusCodes.Status403Forbidden,

            NotFoundException =>
                StatusCodes.Status404NotFound,

            ConflictException =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = statusCode ==
                      StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}