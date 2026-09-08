using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Data.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        HotelReservationDbContext context,
        IPasswordHasher passwordHasher,
        string email,
        string password)
    {
        string normalizedEmail =
            email.Trim().ToLowerInvariant();

        bool exists = await context.Accounts
            .AnyAsync(a => a.Email == normalizedEmail);

        if (exists)
        {
            return;
        }

        var admin = new Admin
        {
            FirstName = "System",
            LastName = "Admin",
            Email = normalizedEmail,
            PasswordHash = passwordHasher.Hash(password),
            IsActive = true
        };

        await context.Accounts.AddAsync(admin);

        await context.SaveChangesAsync();
    }
}