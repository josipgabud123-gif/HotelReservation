using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces;

public interface IAvailabilityBlockRepository
{
    Task<AvailabilityBlock?> GetByIdAsync(int id);

    Task<List<AvailabilityBlock>> GetByAccommodationIdAsync(
        int accommodationId);

    Task<List<AvailabilityBlock>> GetByAccommodationUnitIdAsync(
        int accommodationUnitId);

    Task<bool> HasOverlappingBlockAsync(
        int? accommodationId,
        int? accommodationUnitId,
        DateOnly startDate,
        DateOnly endDate,
        int? excludedBlockId = null);

    Task<bool> IsUnitBlockedAsync(
        int accommodationId,
        int accommodationUnitId,
        DateOnly startDate,
        DateOnly endDate);

    Task AddAsync(AvailabilityBlock availabilityBlock);

    void Remove(AvailabilityBlock availabilityBlock);
}