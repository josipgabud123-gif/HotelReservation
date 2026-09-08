using HotelReservation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelReservation.Infrastructure.Data.Configurations;

public class AccommodationConfiguration
    : IEntityTypeConfiguration<Accommodation>
{
    public void Configure(EntityTypeBuilder<Accommodation> builder)
    {
        builder.ToTable("Accommodations");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(a => a.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(a => a.IsActive)
            .IsRequired();

        builder.HasOne(a => a.Owner)
            .WithMany(o => o.Accommodations)
            .HasForeignKey(a => a.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OwnerId);

        builder.HasIndex(a => new
        {
            a.City,
            a.Country
        });

        builder.HasIndex(a => new
        {
            a.ApprovalStatus,
            a.IsActive
        });
    }
}