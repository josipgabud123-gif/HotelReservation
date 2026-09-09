using HotelReservation.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HotelReservation.Domain.Entities;

public abstract class Account
{
    protected Account(UserRole role)
    {
        Role = role;
    }

    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public UserRole Role { get; }

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();

    public ICollection<Review> Reviews { get; set; }
        = new List<Review>();

    public ICollection<Reservation> CancelledReservations { get; set; }
        = new List<Reservation>();
}
