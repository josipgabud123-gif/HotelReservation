using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.AccommodationUnit;

public class AccommodationUnitDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }

    public bool IsActive { get; set; }

    public int AccommodationId { get; set; }

    public List<AccommodationUnitPhotoDto> Photos { get; set; }
        = new();
}