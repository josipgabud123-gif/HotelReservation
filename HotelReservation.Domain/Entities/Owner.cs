using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class Owner : Account
{
    public Owner()
        : base(UserRole.Owner)
    {
    }

    public ICollection<Accommodation> Accommodations { get; set; }
        = new List<Accommodation>();

    public ICollection<AvailabilityBlock> CreatedAvailabilityBlocks { get; set; }
        = new List<AvailabilityBlock>();
}