using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class Admin : Account
{
    public Admin()
        : base(UserRole.Admin)
    {
    }
}
