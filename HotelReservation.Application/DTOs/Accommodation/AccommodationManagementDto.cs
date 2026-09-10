using HotelReservation.Application.DTOs.AccommodationUnit;
using HotelReservation.Domain.Enums;

namespace HotelReservation.Application.DTOs.Accommodation;

public class AccommodationManagementDto
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

    public List<AccommodationPhotoDto> Photos { get; set; } = new();

    public List<int> AmenityIds { get; set; } = new();

    public List<AccommodationManagementUnitDto> Units { get; set; } = new();
}

public class AccommodationManagementUnitDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public decimal PricePerNight { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; }

    public bool IsActive { get; set; }

    public List<AccommodationUnitPhotoDto> Photos { get; set; } = new();

    public List<int> AmenityIds { get; set; } = new();
}
