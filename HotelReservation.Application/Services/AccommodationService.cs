using HotelReservation.Application.DTOs.Accommodation;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AccommodationService : IAccommodationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public AccommodationService(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }
    public async Task<AccommodationDto> CreateAsync(
    int ownerId,
    CreateAccommodationDto dto)
    {
        Account? account =
            await _unitOfWork.Accounts.GetByIdAsync(ownerId);

        if (account is not Owner)
        {
            throw new ForbiddenException(
                "Only Owner accounts can create accommodations.");
        }

        if (!account.IsActive)
        {
            throw new ForbiddenException(
                "Inactive accounts cannot create accommodations.");
        }

        ValidateBasicData(dto);

        var accommodation = new Domain.Entities.Accommodation
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Address = dto.Address.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),

            Type = dto.Type,

            ApprovalStatus = ApprovalStatus.Draft,

            IsActive = false,

            OwnerId = ownerId,

            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (dto.Type == AccommodationType.Apartment)
        {
            if (!dto.Capacity.HasValue ||
                dto.Capacity.Value <= 0)
            {
                throw new ValidationException(
                    "Apartment capacity must be greater than zero.");
            }

            if (!dto.PricePerNight.HasValue ||
                dto.PricePerNight.Value <= 0)
            {
                throw new ValidationException(
                    "Apartment price per night must be greater than zero.");
            }

            if (!dto.CancellationPolicy.HasValue)
            {
                throw new ValidationException(
                    "Apartment cancellation policy is required.");
            }

            accommodation.Units.Add(
                new AccommodationUnit
                {
                    Name = "Apartment",
                    Capacity = dto.Capacity.Value,
                    PricePerNight = dto.PricePerNight.Value,
                    CancellationPolicy =
                        dto.CancellationPolicy.Value,

                    IsActive = false
                });
        }

        await _unitOfWork.Accommodations.AddAsync(accommodation);

        await _unitOfWork.SaveChangesAsync();

        return Map(accommodation);
    }
    private static void ValidateBasicData(
    CreateAccommodationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException(
                "Accommodation name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException(
                "Description is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Address))
        {
            throw new ValidationException(
                "Address is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.City))
        {
            throw new ValidationException(
                "City is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Country))
        {
            throw new ValidationException(
                "Country is required.");
        }

        if (dto.Type != AccommodationType.Hotel &&
            dto.Type != AccommodationType.Apartment)
        {
            throw new ValidationException(
                "Invalid accommodation type.");
        }
    }
    public async Task<List<AccommodationDto>>
    GetOwnerAccommodationsAsync(int ownerId)
    {
        List<Domain.Entities.Accommodation> accommodations =
            await _unitOfWork.Accommodations
                .GetByOwnerIdAsync(ownerId);

        return accommodations
            .Select(Map)
            .ToList();
    }
    public async Task<AccommodationManagementDto> GetManagementAsync(
        int ownerId,
        int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        return MapManagement(accommodation);
    }

    public async Task<AccommodationManagementDto> GetAdminDetailsAsync(
        int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        return MapManagement(accommodation);
    }

    public async Task<List<AccommodationDto>> GetAllAsync()
    {
        List<Domain.Entities.Accommodation> accommodations =
            await _unitOfWork.Accommodations
                .GetAllAsync();

        return accommodations
            .Where(a => a.ApprovalStatus != ApprovalStatus.Draft)
            .OrderBy(a => a.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<AccommodationDto> UpdateAsync(
    int ownerId,
    int accommodationId,
    UpdateAccommodationDto dto)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Pending accommodation cannot be edited.");
        }

        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Suspended accommodation cannot be edited.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException(
                "Name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException(
                "Description is required.");
        }

        accommodation.Name = dto.Name.Trim();

        accommodation.Description =
            dto.Description.Trim();

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return Map(accommodation);
    }
    public async Task SubmitForApprovalAsync(
    int ownerId,
    int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        if (accommodation.ApprovalStatus != ApprovalStatus.Draft &&
            accommodation.ApprovalStatus != ApprovalStatus.Rejected)
        {
            throw new ConflictException(
                "Only Draft or Rejected accommodations can be submitted.");
        }

        ValidateForSubmission(accommodation);

        accommodation.ApprovalStatus =
            ApprovalStatus.Pending;

        accommodation.IsActive = false;

        accommodation.RejectionReason = null;

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    private static void ValidateForSubmission(
    Domain.Entities.Accommodation accommodation)
    {
        if (string.IsNullOrWhiteSpace(accommodation.Name) ||
            string.IsNullOrWhiteSpace(accommodation.Description) ||
            string.IsNullOrWhiteSpace(accommodation.Address) ||
            string.IsNullOrWhiteSpace(accommodation.City) ||
            string.IsNullOrWhiteSpace(accommodation.Country))
        {
            throw new ValidationException(
                "Accommodation information is incomplete.");
        }

        if (accommodation.Photos.Count == 0)
        {
            throw new ValidationException(
                "Accommodation must have at least one photo.");
        }

        if (accommodation.Photos.Count(p => p.IsPrimary) != 1)
        {
            throw new ValidationException(
                "Accommodation must have exactly one primary photo.");
        }

        if (accommodation.Type == AccommodationType.Hotel)
        {
            if (accommodation.Units.Count == 0)
            {
                throw new ValidationException(
                    "Hotel must have at least one unit.");
            }

            foreach (AccommodationUnit unit in accommodation.Units)
            {
                ValidateHotelUnit(unit);
            }
        }

        if (accommodation.Type ==
            AccommodationType.Apartment)
        {
            if (accommodation.Units.Count != 1)
            {
                throw new ValidationException(
                    "Apartment must contain exactly one unit.");
            }

            AccommodationUnit unit =
                accommodation.Units.Single();

            if (unit.Capacity <= 0 ||
                unit.PricePerNight <= 0)
            {
                throw new ValidationException(
                    "Apartment information is incomplete.");
            }

            // Apartment unit intentionally
            // does NOT require UnitPhoto.
        }
    }
    private static void ValidateHotelUnit(
    AccommodationUnit unit)
    {
        if (string.IsNullOrWhiteSpace(unit.Name))
        {
            throw new ValidationException(
                "All hotel units must have a name.");
        }

        if (unit.Capacity <= 0)
        {
            throw new ValidationException(
                "All hotel units must have a valid capacity.");
        }

        if (unit.PricePerNight <= 0)
        {
            throw new ValidationException(
                "All hotel units must have a valid price.");
        }

        if (unit.Photos.Count == 0)
        {
            throw new ValidationException(
                $"Unit '{unit.Name}' must have at least one photo.");
        }

        if (unit.Photos.Count(p => p.IsPrimary) != 1)
        {
            throw new ValidationException(
                $"Unit '{unit.Name}' must have exactly one primary photo.");
        }
    }
    public async Task<List<AccommodationDto>> GetPendingAsync()
    {
        List<Domain.Entities.Accommodation> accommodations =
            await _unitOfWork.Accommodations
                .GetPendingAsync();

        return accommodations
            .Select(Map)
            .ToList();
    }
    public async Task ApproveAsync(int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        if (accommodation.ApprovalStatus !=
            ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Only Pending accommodations can be approved.");
        }

        ValidateForSubmission(accommodation);

        accommodation.ApprovalStatus =
            ApprovalStatus.Approved;

        accommodation.IsActive = true;

        foreach (AccommodationUnit unit
                 in accommodation.Units)
        {
            unit.IsActive = true;
        }

        accommodation.RejectionReason = null;

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task RejectAsync(
    int accommodationId,
    string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ValidationException(
                "Rejection reason is required.");
        }

        var accommodation =
            await _unitOfWork.Accommodations
                .GetByIdAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        if (accommodation.ApprovalStatus != ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Only Pending accommodations can be rejected.");
        }

        accommodation.ApprovalStatus = ApprovalStatus.Rejected;
        accommodation.IsActive = false;
        accommodation.RejectionReason = reason.Trim();
        accommodation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task ActivateAsync(
    int ownerId,
    int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        if (accommodation.ApprovalStatus !=
            ApprovalStatus.Approved)
        {
            throw new ConflictException(
                "Only Approved accommodation can be activated.");
        }

        accommodation.IsActive = true;

        foreach (AccommodationUnit unit
                 in accommodation.Units)
        {
            unit.IsActive = true;
        }

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task DeactivateAsync(
    int ownerId,
    int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        if (accommodation.ApprovalStatus !=
            ApprovalStatus.Approved)
        {
            throw new ConflictException(
                "Only Approved accommodation can be deactivated.");
        }

        accommodation.IsActive = false;

        foreach (AccommodationUnit unit
                 in accommodation.Units)
        {
            unit.IsActive = false;
        }

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task SuspendAsync(int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        if (accommodation.ApprovalStatus !=
            ApprovalStatus.Approved)
        {
            throw new ConflictException(
                "Only Approved accommodation can be suspended.");
        }

        accommodation.ApprovalStatus =
            ApprovalStatus.Suspended;

        // NAMJERNO NE DIRAMO IsActive.

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task UnsuspendAsync(int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        if (accommodation.ApprovalStatus !=
            ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Only Suspended accommodation can be unsuspended.");
        }

        accommodation.ApprovalStatus =
            ApprovalStatus.Approved;

        // Opet NE mijenjamo IsActive.

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
    public async Task DeleteAsync(
    int ownerId,
    int accommodationId)
    {
        Domain.Entities.Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Pending accommodation cannot be deleted.");
        }

        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Suspended accommodation cannot be deleted.");
        }

        bool hasReservations =
            await _unitOfWork.Accommodations
                .HasAnyReservationAsync(accommodationId);

        if (hasReservations)
        {
            throw new ConflictException(
                "Accommodation that has reservation history cannot be deleted.");
        }

        // AvailabilityBlocks use Restrict,
        // therefore they need to be removed first.

        List<AvailabilityBlock> accommodationBlocks =
            await _unitOfWork.AvailabilityBlocks
                .GetByAccommodationIdAsync(accommodationId);

        foreach (AvailabilityBlock block
                 in accommodationBlocks)
        {
            _unitOfWork.AvailabilityBlocks.Remove(block);
        }

        foreach (AccommodationUnit unit
                 in accommodation.Units)
        {
            List<AvailabilityBlock> unitBlocks =
                await _unitOfWork.AvailabilityBlocks
                    .GetByAccommodationUnitIdAsync(unit.Id);

            foreach (AvailabilityBlock block
                     in unitBlocks)
            {
                _unitOfWork.AvailabilityBlocks.Remove(block);
            }
        }

        _unitOfWork.Accommodations.Remove(accommodation);

        await _unitOfWork.SaveChangesAsync();
    }
    private static void EnsureOwnership(
    Domain.Entities.Accommodation accommodation,
    int ownerId)
    {
        if (accommodation.OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }
    }
    private static AccommodationDto Map(
    Domain.Entities.Accommodation accommodation)
    {
        return new AccommodationDto
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Description = accommodation.Description,
            Address = accommodation.Address,
            City = accommodation.City,
            Country = accommodation.Country,
            Type = accommodation.Type,
            ApprovalStatus =
                accommodation.ApprovalStatus,
            RejectionReason =
                accommodation.RejectionReason,
            IsActive = accommodation.IsActive,
            OwnerId = accommodation.OwnerId,
            CreatedAt = accommodation.CreatedAt,
            UpdatedAt = accommodation.UpdatedAt
        };
    }

    private static AccommodationManagementDto MapManagement(
        Domain.Entities.Accommodation accommodation)
    {
        return new AccommodationManagementDto
        {
            Id = accommodation.Id,
            Name = accommodation.Name,
            Description = accommodation.Description,
            Address = accommodation.Address,
            City = accommodation.City,
            Country = accommodation.Country,
            Type = accommodation.Type,
            ApprovalStatus = accommodation.ApprovalStatus,
            RejectionReason = accommodation.RejectionReason,
            IsActive = accommodation.IsActive,

            Photos = accommodation.Photos
                .OrderByDescending(p => p.IsPrimary)
                .ThenBy(p => p.Id)
                .Select(p => new AccommodationPhotoDto
                {
                    Id = p.Id,
                    Path = p.Path,
                    IsPrimary = p.IsPrimary
                })
                .ToList(),

            AmenityIds = accommodation.AccommodationAmenities
                .Select(aa => aa.AmenityId)
                .OrderBy(id => id)
                .ToList(),

            Units = accommodation.Units
                .OrderBy(u => u.Name)
                .Select(u => new AccommodationManagementUnitDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Capacity = u.Capacity,
                    PricePerNight = u.PricePerNight,
                    CancellationPolicy = u.CancellationPolicy,
                    IsActive = u.IsActive,

                    Photos = u.Photos
                        .OrderByDescending(p => p.IsPrimary)
                        .ThenBy(p => p.Id)
                        .Select(p => new HotelReservation.Application.DTOs.AccommodationUnit.AccommodationUnitPhotoDto
                        {
                            Id = p.Id,
                            Path = p.Path,
                            IsPrimary = p.IsPrimary
                        })
                        .ToList(),

                    AmenityIds = u.AccommodationUnitAmenities
                        .Select(ua => ua.AmenityId)
                        .OrderBy(id => id)
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<AccommodationPhotoDto> AddPhotoAsync(
    int ownerId,
    int accommodationId,
    Stream fileStream,
    string fileName,
    bool isPrimary)
    {
        var accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        EnsureAccommodationCanBeEdited(accommodation);

        bool shouldBePrimary =
            accommodation.Photos.Count == 0 ||
            isPrimary;

        if (shouldBePrimary)
        {
            foreach (var existingPhoto in accommodation.Photos)
            {
                existingPhoto.IsPrimary = false;
            }
        }

        string path = await _fileStorageService.SaveAsync(
            fileStream,
            fileName,
            $"accommodations/{accommodationId}");

        var photo = new AccommodationPhoto
        {
            Path = path,
            IsPrimary = shouldBePrimary,
            AccommodationId = accommodationId
        };

        accommodation.Photos.Add(photo);

        accommodation.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Ako DB spremanje ne uspije,
            // ne želimo ostaviti orphan file na disku.
            _fileStorageService.Delete(path);

            throw;
        }

        return new AccommodationPhotoDto
        {
            Id = photo.Id,
            Path = photo.Path,
            IsPrimary = photo.IsPrimary
        };
    }
    private static void EnsureAccommodationCanBeEdited(
    Domain.Entities.Accommodation accommodation)
    {
        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Pending accommodation cannot be edited.");
        }

        if (accommodation.ApprovalStatus ==
            ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Suspended accommodation cannot be edited.");
        }
    }
    public async Task DeletePhotoAsync(
    int ownerId,
    int accommodationId,
    int photoId)
    {
        var accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        EnsureAccommodationCanBeEdited(accommodation);

        AccommodationPhoto? photo =
            accommodation.Photos
                .FirstOrDefault(p => p.Id == photoId);

        if (photo is null)
        {
            throw new NotFoundException(
                "Photo was not found.");
        }

        if (accommodation.IsActive &&
            accommodation.Photos.Count == 1)
        {
            throw new ConflictException(
                "Active accommodation must have at least one photo.");
        }

        bool wasPrimary = photo.IsPrimary;

        accommodation.Photos.Remove(photo);

        _unitOfWork.Accommodations.RemovePhoto(photo);

        if (wasPrimary &&
            accommodation.Photos.Count > 0)
        {
            accommodation.Photos.First().IsPrimary = true;
        }

        accommodation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        _fileStorageService.Delete(photo.Path);
    }
    public async Task SetPrimaryPhotoAsync(
    int ownerId,
    int accommodationId,
    int photoId)
    {
        var accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnership(accommodation, ownerId);

        EnsureAccommodationCanBeEdited(accommodation);

        AccommodationPhoto? selectedPhoto =
            accommodation.Photos
                .FirstOrDefault(p => p.Id == photoId);

        if (selectedPhoto is null)
        {
            throw new NotFoundException(
                "Photo was not found.");
        }

        foreach (AccommodationPhoto photo in accommodation.Photos)
        {
            photo.IsPrimary =
                photo.Id == selectedPhoto.Id;
        }

        accommodation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }
}