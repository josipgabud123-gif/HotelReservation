using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AmenityConfiguration
    : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        builder.ToTable("Amenities");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.Type)
            .IsUnique();

        builder.HasData(
            Enum.GetValues<AmenityType>()
                .Select(type => new Amenity
                {
                    Id = (int)type,
                    Type = type
                })
        );
    }
}