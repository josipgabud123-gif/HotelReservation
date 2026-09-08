using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class ReviewConfiguration
    : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", table =>
        {
            table.HasCheckConstraint(
                "CK_Review_Rating",
                "[Rating] >= 1 AND [Rating] <= 5");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment)
            .HasMaxLength(2000);

        builder.HasOne(r => r.Account)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Accommodation)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AccommodationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reservation)
            .WithOne(r => r.Review)
            .HasForeignKey<Review>(r => r.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}