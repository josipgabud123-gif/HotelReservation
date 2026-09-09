namespace HotelReservation.Application.DTOs.AccommodationUnit;

public class ImageUploadData
{
    public Stream Stream { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
}