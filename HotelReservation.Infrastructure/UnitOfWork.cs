using HotelReservation.Domain.Interfaces;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;

namespace HotelReservation.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly HotelReservationDbContext _context;

    public UnitOfWork(
        HotelReservationDbContext context,
        IAccountRepository accounts,
        IAccommodationRepository accommodations,
        IAccommodationUnitRepository accommodationUnits,
        IReservationRepository reservations,
        IReviewRepository reviews,
        IAvailabilityBlockRepository availabilityBlocks,
        IAmenityRepository amenities)
    {
        _context = context;

        Accounts = accounts;
        Accommodations = accommodations;
        AccommodationUnits = accommodationUnits;
        Reservations = reservations;
        Reviews = reviews;
        AvailabilityBlocks = availabilityBlocks;
        Amenities = amenities;
    }

    public IAccountRepository Accounts { get; }

    public IAccommodationRepository Accommodations { get; }

    public IAccommodationUnitRepository AccommodationUnits { get; }

    public IReservationRepository Reservations { get; }

    public IReviewRepository Reviews { get; }

    public IAvailabilityBlockRepository AvailabilityBlocks { get; }

    public IAmenityRepository Amenities { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}