using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class AccommodationUnitRepository
    : IAccommodationUnitRepository
{
    private readonly HotelReservationDbContext _context;

    public AccommodationUnitRepository(
        HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<AccommodationUnit?> GetByIdAsync(
        int id)
    {
        return await _context.AccommodationUnits
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AccommodationUnit?>
        GetByIdWithAccommodationAsync(int id)
    {
        return await _context.AccommodationUnits

            .Include(u => u.Accommodation)
                .ThenInclude(a => a.Owner)

            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AccommodationUnit?>
        GetByIdWithDetailsAsync(int id)
    {
        return await _context.AccommodationUnits

            .Include(u => u.Photos)

            .Include(u => u.AccommodationUnitAmenities)
                .ThenInclude(ua => ua.Amenity)

            .Include(u => u.Accommodation)
                .ThenInclude(a => a.Owner)

            .AsSplitQuery()

            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<List<AccommodationUnit>>
        GetByAccommodationIdAsync(
            int accommodationId)
    {
        return await _context.AccommodationUnits

            .Where(u =>
                u.AccommodationId ==
                accommodationId)

            .ToListAsync();
    }

    public async Task<bool> NameExistsAsync(
        int accommodationId,
        string name)
    {
        return await _context.AccommodationUnits
            .AnyAsync(u =>
                u.AccommodationId ==
                    accommodationId &&
                u.Name == name);
    }

    public async Task AddAsync(
        AccommodationUnit unit)
    {
        await _context.AccommodationUnits
            .AddAsync(unit);
    }

    public void Remove(
        AccommodationUnit unit)
    {
        _context.AccommodationUnits
            .Remove(unit);
    }

    public void RemovePhoto(
        AccommodationUnitPhoto photo)
    {
        _context.AccommodationUnitPhotos
            .Remove(photo);
    }

    public void RemoveAmenity(
        AccommodationUnitAmenity
            accommodationUnitAmenity)
    {
        _context.AccommodationUnitAmenities
            .Remove(accommodationUnitAmenity);
    }
}