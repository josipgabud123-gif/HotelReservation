using HotelReservation.Application.DTOs.Amenity;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AmenityService : IAmenityService
{
    private readonly IUnitOfWork _unitOfWork;

    public AmenityService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AmenityDto>> GetAllAsync()
    {
        List<Amenity> amenities =
            await _unitOfWork.Amenities
                .GetAllAsync();

        return amenities
            .OrderBy(a => a.Type)
            .Select(Map)
            .ToList();
    }

    public async Task<List<AmenityDto>>
        SetAccommodationAmenitiesAsync(
            int ownerId,
            int accommodationId,
            UpdateAmenitiesDto dto)
    {
        Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdWithDetailsAsync(
                    accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        EnsureOwnerCanEdit(
            accommodation,
            ownerId);

        HashSet<int> requestedIds =
            dto.AmenityIds
                .Distinct()
                .ToHashSet();

        List<Amenity> requestedAmenities =
            await GetAndValidateAmenitiesAsync(
                requestedIds);

        HashSet<int> currentIds =
            accommodation
                .AccommodationAmenities
                .Select(aa => aa.AmenityId)
                .ToHashSet();

        List<AccommodationAmenity> toRemove =
            accommodation
                .AccommodationAmenities
                .Where(aa =>
                    !requestedIds.Contains(
                        aa.AmenityId))
                .ToList();

        foreach (
            AccommodationAmenity item
            in toRemove)
        {
            accommodation
                .AccommodationAmenities
                .Remove(item);

            _unitOfWork.Accommodations
                .RemoveAmenity(item);
        }

        foreach (
            int amenityId
            in requestedIds)
        {
            if (currentIds.Contains(amenityId))
            {
                continue;
            }

            accommodation
                .AccommodationAmenities
                .Add(
                    new AccommodationAmenity
                    {
                        AccommodationId =
                            accommodationId,

                        AmenityId =
                            amenityId
                    });
        }

        accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return requestedAmenities
            .OrderBy(a => a.Type)
            .Select(Map)
            .ToList();
    }

    public async Task<List<AmenityDto>>
        SetUnitAmenitiesAsync(
            int ownerId,
            int accommodationId,
            int unitId,
            UpdateAmenitiesDto dto)
    {
        AccommodationUnit? unit =
            await _unitOfWork
                .AccommodationUnits
                .GetByIdWithDetailsAsync(
                    unitId);

        if (unit is null ||
            unit.AccommodationId !=
                accommodationId)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        if (unit.Accommodation.OwnerId !=
            ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        if (!unit.Accommodation.Owner.IsActive)
        {
            throw new ForbiddenException(
                "Inactive Owner cannot manage amenities.");
        }

        if (unit.Accommodation.Type !=
            AccommodationType.Hotel)
        {
            throw new ConflictException(
                "Apartment unit cannot have separate amenities.");
        }

        EnsureEditableStatus(
            unit.Accommodation);

        HashSet<int> requestedIds =
            dto.AmenityIds
                .Distinct()
                .ToHashSet();

        List<Amenity> requestedAmenities =
            await GetAndValidateAmenitiesAsync(
                requestedIds);

        HashSet<int> currentIds =
            unit.AccommodationUnitAmenities
                .Select(ua => ua.AmenityId)
                .ToHashSet();

        List<AccommodationUnitAmenity> toRemove =
            unit.AccommodationUnitAmenities
                .Where(ua =>
                    !requestedIds.Contains(
                        ua.AmenityId))
                .ToList();

        foreach (
            AccommodationUnitAmenity item
            in toRemove)
        {
            unit.AccommodationUnitAmenities
                .Remove(item);

            _unitOfWork.AccommodationUnits
                .RemoveAmenity(item);
        }

        foreach (
            int amenityId
            in requestedIds)
        {
            if (currentIds.Contains(amenityId))
            {
                continue;
            }

            unit.AccommodationUnitAmenities
                .Add(
                    new AccommodationUnitAmenity
                    {
                        AccommodationUnitId =
                            unitId,

                        AmenityId =
                            amenityId
                    });
        }

        unit.Accommodation.UpdatedAt =
            DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return requestedAmenities
            .OrderBy(a => a.Type)
            .Select(Map)
            .ToList();
    }

    private async Task<List<Amenity>>
        GetAndValidateAmenitiesAsync(
            HashSet<int> amenityIds)
    {
        if (amenityIds.Count == 0)
        {
            return new List<Amenity>();
        }

        List<Amenity> amenities =
            await _unitOfWork.Amenities
                .GetByIdsAsync(
                    amenityIds.ToList());

        if (amenities.Count !=
            amenityIds.Count)
        {
            throw new ValidationException(
                "One or more amenities do not exist.");
        }

        return amenities;
    }

    private static void EnsureOwnerCanEdit(
        Accommodation accommodation,
        int ownerId)
    {
        if (accommodation.OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        if (!accommodation.Owner.IsActive)
        {
            throw new ForbiddenException(
                "Inactive Owner cannot manage amenities.");
        }

        EnsureEditableStatus(
            accommodation);
    }

    private static void EnsureEditableStatus(
        Accommodation accommodation)
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

    private static AmenityDto Map(
        Amenity amenity)
    {
        return new AmenityDto
        {
            Id = amenity.Id,
            Type = amenity.Type
        };
    }
}