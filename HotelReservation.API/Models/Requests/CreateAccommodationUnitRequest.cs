using HotelReservation.Domain.Enums;

namespace HotelReservation.API.Models.Requests;

public class CreateAccommodationUnitRequest
{
    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }

    public List<IFormFile> Photos { get; set; } = new();

    public int PrimaryPhotoIndex { get; set; } = 0;
}