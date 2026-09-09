using HotelReservation.Application.DTOs.Search;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAccommodationSearchService
{
    Task<List<AccommodationSearchResultDto>> SearchAsync(
        AccommodationSearchDto dto);
}