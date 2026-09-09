using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.AccommodationUnit;

public class UpdateAccommodationUnitDto
{
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }
}