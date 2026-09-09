using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Reservation;

public class ReservationDto
{
    public int Id { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int GuestCount { get; set; }

    public decimal TotalPrice { get; set; }

    public CancellationPolicy CancellationPolicyAtBooking { get; set; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int AccountId { get; set; }

    public int AccommodationUnitId { get; set; }

    public string UnitName { get; set; } = string.Empty;

    public int AccommodationId { get; set; }

    public string AccommodationName { get; set; } = string.Empty;

    public int? CancelledByAccountId { get; set; }
}