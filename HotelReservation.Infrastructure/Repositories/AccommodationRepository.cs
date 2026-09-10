using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class AccommodationRepository : IAccommodationRepository
{
    private readonly HotelReservationDbContext _context;

    public AccommodationRepository(
        HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Accommodation?> GetByIdAsync(int id)
    {
        return await _context.Accommodations
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Accommodation?> GetByIdWithDetailsAsync(
        int id)
    {
        return await _context.Accommodations
            .Include(a => a.Owner)
            .Include(a => a.Photos)

            .Include(a => a.Units)
                .ThenInclude(u => u.Photos)

            .Include(a => a.AccommodationAmenities)
                .ThenInclude(aa => aa.Amenity)

            .Include(a => a.Units)
                .ThenInclude(u => u.AccommodationUnitAmenities)
                .ThenInclude(ua => ua.Amenity)

            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Accommodation>> GetByOwnerIdAsync(
        int ownerId)
    {
        return await _context.Accommodations
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<List<Accommodation>> GetPendingAsync()
    {
        return await _context.Accommodations
            .Include(a => a.Owner)
            .Where(a =>
                a.ApprovalStatus == ApprovalStatus.Pending)
            .ToListAsync();
    }

    public async Task<List<Accommodation>> GetAllAsync()
    {
        return await _context.Accommodations
            .ToListAsync();
    }

    public async Task<List<Accommodation>> GetSearchCandidatesAsync(
        string city,
        string country,
        AccommodationType? type,
        string? nameSearch)
    {
        string normalizedCity =
            city.Trim().ToLower();

        string normalizedCountry =
            country.Trim().ToLower();

        IQueryable<Accommodation> query =
            _context.Accommodations
                .AsNoTracking()

                .Include(a => a.Owner)
                .Include(a => a.Photos)

                .Include(a => a.AccommodationAmenities)
                    .ThenInclude(aa => aa.Amenity)

                .Include(a => a.AvailabilityBlocks)

                .Include(a => a.Units)
                    .ThenInclude(u => u.AccommodationUnitAmenities)
                    .ThenInclude(ua => ua.Amenity)

                .Include(a => a.Units)
                    .ThenInclude(u => u.Photos)

                .Include(a => a.Units)
                    .ThenInclude(u => u.Reservations)

                .Include(a => a.Units)
                    .ThenInclude(u => u.AvailabilityBlocks)

                .AsSplitQuery()

                .Where(a =>
                    a.City.ToLower() == normalizedCity &&
                    a.Country.ToLower() == normalizedCountry);

        if (type.HasValue)
        {
            query = query.Where(a =>
                a.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(nameSearch))
        {
            string normalizedName =
                nameSearch.Trim().ToLower();

            query = query.Where(a =>
                a.Name.ToLower().Contains(normalizedName));
        }

        return await query.ToListAsync();
    }

    public async Task<bool> HasAnyReservationAsync(
        int accommodationId)
    {
        return await _context.Reservations
            .AnyAsync(r =>
                r.AccommodationUnit.AccommodationId ==
                accommodationId);
    }

    public async Task AddAsync(
        Accommodation accommodation)
    {
        await _context.Accommodations
            .AddAsync(accommodation);
    }

    public void Remove(
        Accommodation accommodation)
    {
        _context.Accommodations
            .Remove(accommodation);
    }

    public void RemovePhoto(
        AccommodationPhoto photo)
    {
        _context.AccommodationPhotos
            .Remove(photo);
    }

    public void RemoveAmenity(
        AccommodationAmenity accommodationAmenity)
    {
        _context.AccommodationAmenities
            .Remove(accommodationAmenity);
    }
}