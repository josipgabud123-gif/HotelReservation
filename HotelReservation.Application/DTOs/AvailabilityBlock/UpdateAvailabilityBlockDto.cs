namespace HotelReservation.Application.DTOs.AvailabilityBlock;

public class UpdateAvailabilityBlockDto
{
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}