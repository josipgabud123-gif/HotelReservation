using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservation.Domain.Entities;

public class AvailabilityBlock
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? AccommodationId { get; set; }

    public Accommodation? Accommodation { get; set; }

    public int? AccommodationUnitId { get; set; }

    public AccommodationUnit? AccommodationUnit { get; set; }

    public int CreatedByOwnerId { get; set; }

    public Owner CreatedByOwner { get; set; } = null!;
}

