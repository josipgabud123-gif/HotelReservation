namespace HotelReservation.Application.DTOs.AvailabilityBlock;

public class CreateAvailabilityBlockDto
{
    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }
}