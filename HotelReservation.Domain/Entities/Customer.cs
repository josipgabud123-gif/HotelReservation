using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Entities;

public class Customer : Account
{
    public Customer()
        : base(UserRole.Customer)
    {
    }
}
