using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HotelReservation.Domain.Entities;

public class Review
{
    public int Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public int AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public int AccommodationId { get; set; }

    public Accommodation Accommodation { get; set; } = null!;

    public int ReservationId { get; set; }

    public Reservation Reservation { get; set; } = null!;
}