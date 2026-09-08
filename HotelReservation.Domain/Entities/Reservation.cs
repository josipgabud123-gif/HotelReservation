using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }

    public DateOnly CheckInDate { get; set; }

    public DateOnly CheckOutDate { get; set; }

    public int GuestCount { get; set; }

    public decimal TotalPrice { get; set; }

    public CancellationPolicy CancellationPolicyAtBooking { get; set; }

    public ReservationStatus Status { get; set; }
        = ReservationStatus.Confirmed;

    public DateTime CreatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public int AccommodationUnitId { get; set; }

    public AccommodationUnit AccommodationUnit { get; set; } = null!;

    public int? CancelledByAccountId { get; set; }

    public Account? CancelledByAccount { get; set; }

    public Review? Review { get; set; }
}