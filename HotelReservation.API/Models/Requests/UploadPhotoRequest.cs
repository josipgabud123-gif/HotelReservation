namespace HotelReservation.API.Models.Requests;

public class UploadPhotoRequest
{
    public IFormFile File { get; set; } = null!;

    public bool IsPrimary { get; set; }
}