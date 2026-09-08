using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HotelReservation.Domain.Entities;

public class AccommodationUnitAmenity
{
    public int AccommodationUnitId { get; set; }

    public AccommodationUnit AccommodationUnit { get; set; } = null!;

    public int AmenityId { get; set; }

    public Amenity Amenity { get; set; } = null!;
}