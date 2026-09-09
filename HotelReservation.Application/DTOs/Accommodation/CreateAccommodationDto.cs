using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Accommodation;

public class CreateAccommodationDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public AccommodationType Type { get; set; }

    // Koristi se samo ako je Type = Apartment

    public int? Capacity { get; set; }

    public decimal? PricePerNight { get; set; }

    public CancellationPolicy? CancellationPolicy { get; set; }
}