namespace HotelReservation.Application.DTOs.AvailabilityBlock;

public class AvailabilityBlockDto
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? AccommodationId { get; set; }

    public int? AccommodationUnitId { get; set; }

    public int CreatedByOwnerId { get; set; }
}