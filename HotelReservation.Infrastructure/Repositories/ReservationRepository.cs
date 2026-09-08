using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly HotelReservationDbContext _context;

    public ReservationRepository(HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Reservation?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Reservations
            .Include(r => r.Account)
            .Include(r => r.AccommodationUnit)
                .ThenInclude(u => u.Accommodation)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Reservation>> GetByAccountIdAsync(int accountId)
    {
        return await _context.Reservations
            .Include(r => r.AccommodationUnit)
                .ThenInclude(u => u.Accommodation)
            .Where(r => r.AccountId == accountId)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetByOwnerIdAsync(int ownerId)
    {
        return await _context.Reservations
            .Include(r => r.Account)
            .Include(r => r.AccommodationUnit)
                .ThenInclude(u => u.Accommodation)
            .Where(r =>
                r.AccommodationUnit.Accommodation.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .Include(r => r.Account)
            .Include(r => r.AccommodationUnit)
                .ThenInclude(u => u.Accommodation)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingConfirmedReservationAsync(
        int accommodationUnitId,
        DateOnly checkInDate,
        DateOnly checkOutDate)
    {
        return await _context.Reservations.AnyAsync(r =>
            r.AccommodationUnitId == accommodationUnitId &&
            r.Status == ReservationStatus.Confirmed &&
            checkInDate < r.CheckOutDate &&
            checkOutDate > r.CheckInDate);
    }

    public async Task<bool> UnitHasAnyReservationAsync(
        int accommodationUnitId)
    {
        return await _context.Reservations
            .AnyAsync(r => r.AccommodationUnitId == accommodationUnitId);
    }

    public async Task<List<Reservation>>
        GetConfirmedReservationsForCompletionAsync(DateOnly today)
    {
        return await _context.Reservations
            .Where(r =>
                r.Status == ReservationStatus.Confirmed &&
                r.CheckOutDate <= today)
            .ToListAsync();
    }

    public async Task AddAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
    }
}