using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Search;

public class AccommodationSearchResultDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public AccommodationType Type { get; set; }

    public string? PrimaryPhotoPath { get; set; }

    public decimal FromPricePerNight { get; set; }

    public decimal FromTotalPrice { get; set; }

    public List<AvailableUnitDto> AvailableUnits { get; set; } = new();
}