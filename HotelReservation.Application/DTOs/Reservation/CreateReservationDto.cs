namespace HotelReservation.Application.DTOs.Reservation;

public class CreateReservationDto
{
    public int AccommodationUnitId { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int GuestCount { get; set; }
}