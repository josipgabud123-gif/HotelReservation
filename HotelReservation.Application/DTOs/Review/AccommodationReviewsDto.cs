namespace HotelReservation.Application.DTOs.Review;

public class AccommodationReviewsDto
{
    public int AccommodationId { get; set; }

    public double? AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public List<ReviewDto> Reviews { get; set; } = new();
}