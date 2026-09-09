using HotelReservation.Application.DTOs.Amenity;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAmenityService
{
    Task<List<AmenityDto>> GetAllAsync();

    Task<List<AmenityDto>> SetAccommodationAmenitiesAsync(
        int ownerId,
        int accommodationId,
        UpdateAmenitiesDto dto);

    Task<List<AmenityDto>> SetUnitAmenitiesAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        UpdateAmenitiesDto dto);
}