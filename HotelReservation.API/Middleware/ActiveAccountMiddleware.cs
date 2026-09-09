using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces;
using HotelReservation.Domain.Interfaces.Repositories;
using System.Security.Claims;

namespace HotelReservation.API.Middleware;

public class ActiveAccountMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveAccountMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAccountRepository accountRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            string? accountIdValue =
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    accountIdValue,
                    out int accountId))
            {
                throw new UnauthorizedException(
                    "Invalid authentication token.");
            }

            Account? account =
                await accountRepository
                    .GetByIdAsync(accountId);

            if (account is null ||
                !account.IsActive)
            {
                throw new UnauthorizedException(
                    "Account is inactive or no longer exists.");
            }
        }

        await _next(context);
    }
}