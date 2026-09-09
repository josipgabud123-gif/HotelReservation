using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Accommodation;

public class AccommodationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public AccommodationType Type { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public string? RejectionReason { get; set; }

    public bool IsActive { get; set; }

    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}