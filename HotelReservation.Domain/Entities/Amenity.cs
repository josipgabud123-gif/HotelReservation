using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class Amenity
{
    public int Id { get; set; }

    public AmenityType Type { get; set; }

    public ICollection<AccommodationAmenity> AccommodationAmenities { get; set; }
        = new List<AccommodationAmenity>();

    public ICollection<AccommodationUnitAmenity> AccommodationUnitAmenities { get; set; }
        = new List<AccommodationUnitAmenity>();
}