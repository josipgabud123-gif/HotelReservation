namespace HotelReservation.Application.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int AccountId { get; set; }

    public string ReviewerName { get; set; } = string.Empty;

    public int AccommodationId { get; set; }

    public int ReservationId { get; set; }
}