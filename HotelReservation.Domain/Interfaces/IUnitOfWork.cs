using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelReservation.Domain.Interfaces.Repositories;

namespace HotelReservation.Domain.Interfaces;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }

    IAccommodationRepository Accommodations { get; }

    IAccommodationUnitRepository AccommodationUnits { get; }

    IReservationRepository Reservations { get; }

    IReviewRepository Reviews { get; }

    IAvailabilityBlockRepository AvailabilityBlocks { get; }

    IAmenityRepository Amenities { get; }

    Task<int> SaveChangesAsync();
}