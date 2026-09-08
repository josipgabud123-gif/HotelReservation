using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationUnitConfiguration
    : IEntityTypeConfiguration<AccommodationUnit>
{
    public void Configure(EntityTypeBuilder<AccommodationUnit> builder)
    {
        builder.ToTable("AccommodationUnits", table =>
        {
            table.HasCheckConstraint(
                "CK_AccommodationUnit_Capacity",
                "[Capacity] > 0");

            table.HasCheckConstraint(
                "CK_AccommodationUnit_PricePerNight",
                "[PricePerNight] > 0");
        });

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.PricePerNight)
            .HasPrecision(12, 2);

        builder.Property(u => u.IsActive)
            .IsRequired();

        builder.HasOne(u => u.Accommodation)
            .WithMany(a => a.Units)
            .HasForeignKey(u => u.AccommodationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => new
        {
            u.AccommodationId,
            u.Name
        })
        .IsUnique();
    }
}