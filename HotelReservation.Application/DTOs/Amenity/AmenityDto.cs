using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Amenity;

public class AmenityDto
{
    public int Id { get; set; }

    public AmenityType Type { get; set; }
}