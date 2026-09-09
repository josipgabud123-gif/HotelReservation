namespace HotelReservation.Application.DTOs.AccommodationUnit;

public class AccommodationUnitPhotoDto
{
    public int Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}