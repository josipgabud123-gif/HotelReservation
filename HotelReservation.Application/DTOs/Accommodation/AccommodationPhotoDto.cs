namespace HotelReservation.Application.DTOs.Accommodation;

public class AccommodationPhotoDto
{
    public int Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}