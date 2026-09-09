using HotelReservation.Application.DTOs.AvailabilityBlock;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAvailabilityBlockService
{
    Task<AvailabilityBlockDto> CreateForAccommodationAsync(
        int ownerId,
        int accommodationId,
        CreateAvailabilityBlockDto dto);

    Task<AvailabilityBlockDto> CreateForUnitAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        CreateAvailabilityBlockDto dto);

    Task<List<AvailabilityBlockDto>>
        GetAccommodationBlocksAsync(
            int ownerId,
            int accommodationId,
            bool includePast);

    Task<List<AvailabilityBlockDto>>
        GetUnitBlocksAsync(
            int ownerId,
            int accommodationId,
            int unitId,
            bool includePast);

    Task<AvailabilityBlockDto> UpdateAsync(
        int ownerId,
        int blockId,
        UpdateAvailabilityBlockDto dto);

    Task DeleteAsync(
        int ownerId,
        int blockId);
}