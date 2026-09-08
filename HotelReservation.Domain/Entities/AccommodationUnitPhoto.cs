using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservation.Domain.Entities;

public class AccommodationUnitPhoto
{
    public int Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int AccommodationUnitId { get; set; }

    public AccommodationUnit AccommodationUnit { get; set; } = null!;
}
