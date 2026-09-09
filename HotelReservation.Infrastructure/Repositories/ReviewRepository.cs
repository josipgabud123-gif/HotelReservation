using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly HotelReservationDbContext _context;

    public ReviewRepository(
        HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByIdAsync(
        int id)
    {
        return await _context.Reviews
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r =>
                r.Id == id);
    }

    public async Task<Review?>
        GetByReservationIdAsync(
            int reservationId)
    {
        return await _context.Reviews
            .Include(r => r.Account)
            .FirstOrDefaultAsync(r =>
                r.ReservationId ==
                reservationId);
    }

    public async Task<List<Review>>
        GetByAccommodationIdAsync(
            int accommodationId)
    {
        return await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Account)

            .Where(r =>
                r.AccommodationId ==
                accommodationId)

            .OrderByDescending(r =>
                r.CreatedAt)

            .ToListAsync();
    }

    public async Task AddAsync(
        Review review)
    {
        await _context.Reviews
            .AddAsync(review);
    }

    public void Remove(
        Review review)
    {
        _context.Reviews
            .Remove(review);
    }
}