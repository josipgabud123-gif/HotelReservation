using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class AvailabilityBlockRepository
    : IAvailabilityBlockRepository
{
    private readonly HotelReservationDbContext _context;

    public AvailabilityBlockRepository(
        HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityBlock?> GetByIdAsync(int id)
    {
        return await _context.AvailabilityBlocks
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<AvailabilityBlock>>
        GetByAccommodationIdAsync(int accommodationId)
    {
        return await _context.AvailabilityBlocks
            .Where(b => b.AccommodationId == accommodationId)
            .ToListAsync();
    }

    public async Task<List<AvailabilityBlock>>
        GetByAccommodationUnitIdAsync(int accommodationUnitId)
    {
        return await _context.AvailabilityBlocks
            .Where(b => b.AccommodationUnitId == accommodationUnitId)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingBlockAsync(
        int? accommodationId,
        int? accommodationUnitId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludedBlockId = null)
    {
        return await _context.AvailabilityBlocks.AnyAsync(b =>
            (!excludedBlockId.HasValue || b.Id != excludedBlockId.Value) &&
            b.AccommodationId == accommodationId &&
            b.AccommodationUnitId == accommodationUnitId &&
            startDate < b.EndDate &&
            endDate > b.StartDate);
    }

    public async Task<bool> IsUnitBlockedAsync(
        int accommodationId,
        int accommodationUnitId,
        DateOnly checkInDate,
        DateOnly checkOutDate)
    {
        return await _context.AvailabilityBlocks.AnyAsync(b =>
            (
                b.AccommodationId == accommodationId ||
                b.AccommodationUnitId == accommodationUnitId
            )
            &&
            checkInDate < b.EndDate &&
            checkOutDate > b.StartDate);
    }

    public async Task AddAsync(AvailabilityBlock availabilityBlock)
    {
        await _context.AvailabilityBlocks.AddAsync(availabilityBlock);
    }

    public void Remove(AvailabilityBlock availabilityBlock)
    {
        _context.AvailabilityBlocks.Remove(availabilityBlock);
    }

    public void RemoveRange(IEnumerable<AvailabilityBlock> blocks)
    {
        _context.AvailabilityBlocks.RemoveRange(blocks);
    }

}