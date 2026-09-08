using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Data;

public class HotelReservationDbContext : DbContext
{
    public HotelReservationDbContext(
        DbContextOptions<HotelReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Accommodation> Accommodations { get; set; }
    public DbSet<AccommodationUnit> AccommodationUnits { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<AccommodationAmenity> AccommodationAmenities { get; set; }
    public DbSet<AccommodationUnitAmenity> AccommodationUnitAmenities { get; set; }
    public DbSet<AccommodationPhoto> AccommodationPhotos { get; set; }
    public DbSet<AccommodationUnitPhoto> AccommodationUnitPhotos { get; set; }
    public DbSet<AvailabilityBlock> AvailabilityBlocks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(HotelReservationDbContext).Assembly);
    }
}