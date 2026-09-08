using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class ReservationConfiguration
    : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", table =>
        {
            table.HasCheckConstraint(
                "CK_Reservation_Dates",
                "[CheckOutDate] > [CheckInDate]");

            table.HasCheckConstraint(
                "CK_Reservation_GuestCount",
                "[GuestCount] > 0");

            table.HasCheckConstraint(
                "CK_Reservation_TotalPrice",
                "[TotalPrice] > 0");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TotalPrice)
            .HasPrecision(12, 2);

        builder.HasOne(r => r.Account)
            .WithMany(a => a.Reservations)
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CancelledByAccount)
            .WithMany(a => a.CancelledReservations)
            .HasForeignKey(r => r.CancelledByAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AccommodationUnit)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.AccommodationUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new
        {
            r.AccommodationUnitId,
            r.Status,
            r.CheckInDate,
            r.CheckOutDate
        });
    }
}