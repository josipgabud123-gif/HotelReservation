using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Search;

public class AvailableUnitDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public decimal TotalPrice { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }

    public List<int> AmenityIds { get; set; } = new();
}