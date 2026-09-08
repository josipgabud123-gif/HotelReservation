using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationUnitPhotoConfiguration
    : IEntityTypeConfiguration<AccommodationUnitPhoto>
{
    public void Configure(EntityTypeBuilder<AccommodationUnitPhoto> builder)
    {
        builder.ToTable("AccommodationUnitPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(p => p.AccommodationUnit)
            .WithMany(u => u.Photos)
            .HasForeignKey(p => p.AccommodationUnitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.AccommodationUnitId)
            .HasFilter("[IsPrimary] = 1")
            .IsUnique();
    }
}
