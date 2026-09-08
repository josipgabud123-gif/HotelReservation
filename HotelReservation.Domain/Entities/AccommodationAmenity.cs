using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HotelReservation.Domain.Entities;

public class AccommodationAmenity
{
    public int AccommodationId { get; set; }

    public Accommodation Accommodation { get; set; } = null!;

    public int AmenityId { get; set; }

    public Amenity Amenity { get; set; } = null!;
}