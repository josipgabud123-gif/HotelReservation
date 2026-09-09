using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Search;

public class AccommodationSearchDto
{
    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public int GuestCount { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public List<int> AmenityIds { get; set; } = new();

    public AccommodationType? Type { get; set; }

    public string? NameSearch { get; set; }

    public AccommodationSearchSort Sort { get; set; }
        = AccommodationSearchSort.PriceAscending;
}