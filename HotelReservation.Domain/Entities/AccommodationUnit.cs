using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class AccommodationUnit
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }

    public bool IsActive { get; set; }

    public int AccommodationId { get; set; }

    public Accommodation Accommodation { get; set; } = null!;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();

    public ICollection<AccommodationUnitPhoto> Photos { get; set; }
        = new List<AccommodationUnitPhoto>();

    public ICollection<AccommodationUnitAmenity> AccommodationUnitAmenities { get; set; }
        = new List<AccommodationUnitAmenity>();

    public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; }
        = new List<AvailabilityBlock>();
}