using HotelReservation.Application.DTOs.AccommodationUnit;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AccommodationUnitService : IAccommodationUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public AccommodationUnitService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<AccommodationUnitDto> CreateAsync(
        int ownerId,
        int accommodationId,
        CreateAccommodationUnitDto dto,
        IReadOnlyList<ImageUploadData> photos,
        int primaryPhotoIndex)
    {
        var accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        if (accommodation.OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        if (!accommodation.Owner.IsActive)
        {
            throw new ForbiddenException(
                "Inactive Owner cannot manage accommodation units.");
        }

        if (accommodation.Type != AccommodationType.Hotel)
        {
            throw new ConflictException(
                "Units can only be manually added to hotels.");
        }

        if (accommodation.ApprovalStatus == ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Pending accommodation cannot be edited.");
        }

        if (accommodation.ApprovalStatus == ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Suspended accommodation cannot be edited.");
        }

        ValidateUnit(
            dto,
            photos,
            primaryPhotoIndex);

        string unitName = dto.Name.Trim();

        bool nameExists =
            await _unitOfWork.AccommodationUnits
                .NameExistsAsync(
                    accommodationId,
                    unitName);

        if (nameExists)
        {
            throw new ConflictException(
                "A unit with this name already exists in the accommodation.");
        }

        var unit = new AccommodationUnit
        {
            Name = unitName,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            CancellationPolicy = dto.CancellationPolicy,
            AccommodationId = accommodationId,

            IsActive =
                accommodation.ApprovalStatus ==
                    ApprovalStatus.Approved &&
                accommodation.IsActive
        };

        var savedPaths = new List<string>();

        try
        {
            for (int i = 0; i < photos.Count; i++)
            {
                ImageUploadData uploadedPhoto =
                    photos[i];

                string path =
                    await _fileStorageService.SaveAsync(
                        uploadedPhoto.Stream,
                        uploadedPhoto.FileName,
                        $"accommodations/{accommodationId}/units");

                savedPaths.Add(path);

                unit.Photos.Add(
                    new AccommodationUnitPhoto
                    {
                        Path = path,
                        IsPrimary =
                            i == primaryPhotoIndex
                    });
            }

            await _unitOfWork.AccommodationUnits
                .AddAsync(unit);

            accommodation.UpdatedAt =
                DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            foreach (string path in savedPaths)
            {
                _fileStorageService.Delete(path);
            }

            throw;
        }

        return Map(unit);
    }

    public async Task<AccommodationUnitDto> UpdateAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        UpdateAccommodationUnitDto dto)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        EnsureUnitBelongsToAccommodation(
            unit,
            accommodationId);

        EnsureCanManageUnit(
            unit,
            ownerId);

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException(
                "Unit name is required.");
        }

        if (dto.Capacity <= 0)
        {
            throw new ValidationException(
                "Capacity must be greater than zero.");
        }

        if (dto.PricePerNight <= 0)
        {
            throw new ValidationException(
                "Price per night must be greater than zero.");
        }

        if (!Enum.IsDefined(dto.CancellationPolicy))
        {
            throw new ValidationException(
                "Invalid cancellation policy.");
        }

        string newName =
            dto.Name.Trim();

        bool hasReservations =
            await _unitOfWork.Reservations
                .UnitHasAnyReservationAsync(unitId);

        if (hasReservations &&
            !string.Equals(
                unit.Name,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "Unit name cannot be changed after the unit has had a reservation.");
        }

        if (!string.Equals(
                unit.Name,
                newName,
                StringComparison.OrdinalIgnoreCase))
        {
            bool nameExists =
                await _unitOfWork.AccommodationUnits
                    .NameExistsAsync(
                        accommodationId,
                        newName);

            if (nameExists)
            {
                throw new ConflictException(
                    "A unit with this name already exists in the accommodation.");
            }
        }

        unit.Name = newName;
        unit.Capacity = dto.Capacity;
        unit.PricePerNight =
            dto.PricePerNight;
        unit.CancellationPolicy =
            dto.CancellationPolicy;

        unit.Accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return Map(unit);
    }

    public async Task ActivateAsync(
        int ownerId,
        int accommodationId,
        int unitId)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        EnsureUnitBelongsToAccommodation(
            unit,
            accommodationId);

        EnsureCanManageUnit(
            unit,
            ownerId);

        if (unit.Accommodation.ApprovalStatus !=
            ApprovalStatus.Approved)
        {
            throw new ConflictException(
                "Unit can only be activated in an approved accommodation.");
        }

        if (!unit.Accommodation.IsActive)
        {
            throw new ConflictException(
                "Unit cannot be activated while accommodation is inactive.");
        }

        unit.IsActive = true;

        unit.Accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateAsync(
        int ownerId,
        int accommodationId,
        int unitId)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        EnsureUnitBelongsToAccommodation(
            unit,
            accommodationId);

        EnsureCanManageUnit(
            unit,
            ownerId);

        unit.IsActive = false;

        unit.Accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        int ownerId,
        int accommodationId,
        int unitId)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        EnsureUnitBelongsToAccommodation(
            unit,
            accommodationId);

        EnsureCanManageUnit(
            unit,
            ownerId);

        bool hasReservations =
            await _unitOfWork.Reservations
                .UnitHasAnyReservationAsync(unitId);

        if (hasReservations)
        {
            throw new ConflictException(
                "Unit with reservation history cannot be deleted. Deactivate it instead.");
        }

        List<AccommodationUnit> units =
            await _unitOfWork.AccommodationUnits
                .GetByAccommodationIdAsync(
                    accommodationId);

        if (unit.Accommodation.IsActive &&
            units.Count == 1)
        {
            throw new ConflictException(
                "The last unit of an active hotel cannot be deleted. Deactivate the hotel first.");
        }

        List<AvailabilityBlock> blocks =
            await _unitOfWork.AvailabilityBlocks
                .GetByAccommodationUnitIdAsync(
                    unitId);

        foreach (AvailabilityBlock block in blocks)
        {
            _unitOfWork.AvailabilityBlocks
                .Remove(block);
        }

        List<string> photoPaths =
            unit.Photos
                .Select(p => p.Path)
                .ToList();

        _unitOfWork.AccommodationUnits
            .Remove(unit);

        unit.Accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        foreach (string path in photoPaths)
        {
            _fileStorageService.Delete(path);
        }
    }

    private static void EnsureUnitBelongsToAccommodation(
        AccommodationUnit unit,
        int accommodationId)
    {
        if (unit.AccommodationId !=
            accommodationId)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }
    }

    private static void EnsureCanManageUnit(
        AccommodationUnit unit,
        int ownerId)
    {
        if (unit.Accommodation.OwnerId !=
            ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        if (!unit.Accommodation.Owner.IsActive)
        {
            throw new ForbiddenException(
                "Inactive Owner cannot manage accommodation units.");
        }

        if (unit.Accommodation.Type !=
            AccommodationType.Hotel)
        {
            throw new ConflictException(
                "Apartment unit cannot be managed separately.");
        }

        if (unit.Accommodation.ApprovalStatus ==
            ApprovalStatus.Pending)
        {
            throw new ConflictException(
                "Pending accommodation cannot be edited.");
        }

        if (unit.Accommodation.ApprovalStatus ==
            ApprovalStatus.Suspended)
        {
            throw new ConflictException(
                "Suspended accommodation cannot be edited.");
        }
    }

    private static void ValidateUnit(
        CreateAccommodationUnitDto dto,
        IReadOnlyList<ImageUploadData> photos,
        int primaryPhotoIndex)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ValidationException(
                "Unit name is required.");
        }

        if (dto.Capacity <= 0)
        {
            throw new ValidationException(
                "Capacity must be greater than zero.");
        }

        if (dto.PricePerNight <= 0)
        {
            throw new ValidationException(
                "Price per night must be greater than zero.");
        }

        if (!Enum.IsDefined(
                dto.CancellationPolicy))
        {
            throw new ValidationException(
                "Invalid cancellation policy.");
        }

        if (photos.Count == 0)
        {
            throw new ValidationException(
                "Hotel unit must have at least one photo.");
        }

        if (primaryPhotoIndex < 0 ||
            primaryPhotoIndex >= photos.Count)
        {
            throw new ValidationException(
                "Invalid primary photo index.");
        }
    }

    private static AccommodationUnitDto Map(
        AccommodationUnit unit)
    {
        return new AccommodationUnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Capacity = unit.Capacity,
            PricePerNight =
                unit.PricePerNight,
            CancellationPolicy =
                unit.CancellationPolicy,
            IsActive = unit.IsActive,
            AccommodationId =
                unit.AccommodationId,

            Photos = unit.Photos
                .Select(p =>
                    new AccommodationUnitPhotoDto
                    {
                        Id = p.Id,
                        Path = p.Path,
                        IsPrimary = p.IsPrimary
                    })
                .ToList()
        };
    }
}