using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationAmenityConfiguration
    : IEntityTypeConfiguration<AccommodationAmenity>
{
    public void Configure(EntityTypeBuilder<AccommodationAmenity> builder)
    {
        builder.ToTable("AccommodationAmenities");

        builder.HasKey(aa => new
        {
            aa.AccommodationId,
            aa.AmenityId
        });

        builder.HasOne(aa => aa.Accommodation)
            .WithMany(a => a.AccommodationAmenities)
            .HasForeignKey(aa => aa.AccommodationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(aa => aa.Amenity)
            .WithMany(a => a.AccommodationAmenities)
            .HasForeignKey(aa => aa.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}