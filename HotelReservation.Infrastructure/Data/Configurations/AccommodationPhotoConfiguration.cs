using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationPhotoConfiguration
    : IEntityTypeConfiguration<AccommodationPhoto>
{
    public void Configure(EntityTypeBuilder<AccommodationPhoto> builder)
    {
        builder.ToTable("AccommodationPhotos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(p => p.Accommodation)
            .WithMany(a => a.Photos)
            .HasForeignKey(p => p.AccommodationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.AccommodationId)
            .HasFilter("[IsPrimary] = 1")
            .IsUnique();
    }
}
