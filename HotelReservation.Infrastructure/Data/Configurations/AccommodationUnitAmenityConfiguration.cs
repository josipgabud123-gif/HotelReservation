using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationUnitAmenityConfiguration
    : IEntityTypeConfiguration<AccommodationUnitAmenity>
{
    public void Configure(EntityTypeBuilder<AccommodationUnitAmenity> builder)
    {
        builder.ToTable("AccommodationUnitAmenities");

        builder.HasKey(ua => new
        {
            ua.AccommodationUnitId,
            ua.AmenityId
        });

        builder.HasOne(ua => ua.AccommodationUnit)
            .WithMany(u => u.AccommodationUnitAmenities)
            .HasForeignKey(ua => ua.AccommodationUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Amenity)
            .WithMany(a => a.AccommodationUnitAmenities)
            .HasForeignKey(ua => ua.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}