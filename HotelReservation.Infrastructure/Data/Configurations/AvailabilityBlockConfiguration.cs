using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AvailabilityBlockConfiguration
    : IEntityTypeConfiguration<AvailabilityBlock>
{
    public void Configure(EntityTypeBuilder<AvailabilityBlock> builder)
    {
        builder.ToTable("AvailabilityBlocks", table =>
        {
            table.HasCheckConstraint(
                "CK_AvailabilityBlock_Dates",
                "[EndDate] > [StartDate]");

            table.HasCheckConstraint(
                "CK_AvailabilityBlock_Target",
                @"(
                    [AccommodationId] IS NOT NULL
                    AND [AccommodationUnitId] IS NULL
                  )
                  OR
                  (
                    [AccommodationId] IS NULL
                    AND [AccommodationUnitId] IS NOT NULL
                  )");
        });

        builder.HasKey(b => b.Id);

        builder.HasOne(b => b.Accommodation)
            .WithMany(a => a.AvailabilityBlocks)
            .HasForeignKey(b => b.AccommodationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.AccommodationUnit)
            .WithMany(u => u.AvailabilityBlocks)
            .HasForeignKey(b => b.AccommodationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.CreatedByOwner)
            .WithMany(o => o.CreatedAvailabilityBlocks)
            .HasForeignKey(b => b.CreatedByOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.AccommodationId);

        builder.HasIndex(b => b.AccommodationUnitId);
    }
}