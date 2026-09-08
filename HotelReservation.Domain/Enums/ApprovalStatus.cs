using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservation.Domain.Enums;

public enum ApprovalStatus
{
    Draft = 1,
    Pending = 2,
    Approved = 3,
    Rejected = 4,
    Suspended = 5
}