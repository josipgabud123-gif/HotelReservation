using HotelReservation.Application.DTOs.Search;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AccommodationSearchService
    : IAccommodationSearchService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccommodationSearchService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AccommodationSearchResultDto>>
        SearchAsync(
            AccommodationSearchDto dto)
    {
        ValidateSearch(dto);

        HashSet<int> requestedAmenityIds =
            dto.AmenityIds
                .Distinct()
                .ToHashSet();

        await ValidateAmenitiesAsync(
            requestedAmenityIds);

        List<Accommodation> candidates =
            await _unitOfWork.Accommodations
                .GetSearchCandidatesAsync(
                    dto.City,
                    dto.Country,
                    dto.Type,
                    dto.NameSearch);

        int nights =
            dto.CheckOut.DayNumber -
            dto.CheckIn.DayNumber;

        var results =
            new List<AccommodationSearchResultDto>();

        foreach (Accommodation accommodation in candidates)
        {
            if (!CanAppearInSearch(accommodation))
            {
                continue;
            }

            var availableUnits =
                new List<AvailableUnitDto>();

            foreach (AccommodationUnit unit
                     in accommodation.Units)
            {
                AvailableUnitDto? availableUnit =
                    GetAvailableUnit(
                        accommodation,
                        unit,
                        dto,
                        requestedAmenityIds,
                        nights);

                if (availableUnit is not null)
                {
                    availableUnits.Add(
                        availableUnit);
                }
            }

            if (availableUnits.Count == 0)
            {
                continue;
            }

            availableUnits =
                availableUnits
                    .OrderBy(u => u.PricePerNight)
                    .ToList();

            AccommodationPhoto? primaryPhoto =
                accommodation.Photos
                    .FirstOrDefault(p => p.IsPrimary);

            primaryPhoto ??=
                accommodation.Photos.FirstOrDefault();

            AvailableUnitDto cheapestUnit =
                availableUnits.First();

            results.Add(
                new AccommodationSearchResultDto
                {
                    Id = accommodation.Id,
                    Name = accommodation.Name,
                    Description =
                        accommodation.Description,
                    City = accommodation.City,
                    Country = accommodation.Country,
                    Type = accommodation.Type,
                    PrimaryPhotoPath =
                        primaryPhoto?.Path,

                    FromPricePerNight =
                        cheapestUnit.PricePerNight,

                    FromTotalPrice =
                        cheapestUnit.TotalPrice,

                    AvailableUnits =
                        availableUnits
                });
        }

        return SortResults(
            results,
            dto.Sort);
    }

    private static AvailableUnitDto?
        GetAvailableUnit(
            Accommodation accommodation,
            AccommodationUnit unit,
            AccommodationSearchDto dto,
            HashSet<int> requestedAmenityIds,
            int nights)
    {
        if (!unit.IsActive)
        {
            return null;
        }

        if (unit.Capacity < dto.GuestCount)
        {
            return null;
        }

        bool reservationOverlap =
            unit.Reservations.Any(r =>
                r.Status ==
                    ReservationStatus.Confirmed &&
                dto.CheckIn < r.CheckOutDate &&
                dto.CheckOut > r.CheckInDate);

        if (reservationOverlap)
        {
            return null;
        }

        bool accommodationBlocked =
            accommodation.AvailabilityBlocks.Any(b =>
                dto.CheckIn < b.EndDate &&
                dto.CheckOut > b.StartDate);

        if (accommodationBlocked)
        {
            return null;
        }

        bool unitBlocked =
            unit.AvailabilityBlocks.Any(b =>
                dto.CheckIn < b.EndDate &&
                dto.CheckOut > b.StartDate);

        if (unitBlocked)
        {
            return null;
        }

        decimal totalPrice =
            unit.PricePerNight * nights;

        if (dto.MinPrice.HasValue &&
            totalPrice < dto.MinPrice.Value)
        {
            return null;
        }

        if (dto.MaxPrice.HasValue &&
            totalPrice > dto.MaxPrice.Value)
        {
            return null;
        }

        HashSet<int> effectiveAmenities =
            GetEffectiveAmenityIds(
                accommodation,
                unit);

        bool hasEveryRequestedAmenity =
            requestedAmenityIds.All(id =>
                effectiveAmenities.Contains(id));

        if (!hasEveryRequestedAmenity)
        {
            return null;
        }

        return new AvailableUnitDto
        {
            Id = unit.Id,
            Name = unit.Name,
            Capacity = unit.Capacity,
            PricePerNight =
                unit.PricePerNight,
            TotalPrice =
                totalPrice,
            CancellationPolicy =
                unit.CancellationPolicy,

            AmenityIds =
                effectiveAmenities
                    .OrderBy(id => id)
                    .ToList()
        };
    }

    private static HashSet<int>
        GetEffectiveAmenityIds(
            Accommodation accommodation,
            AccommodationUnit unit)
    {
        HashSet<int> amenityIds =
            accommodation.AccommodationAmenities
                .Select(aa => aa.AmenityId)
                .ToHashSet();

        if (accommodation.Type ==
            AccommodationType.Hotel)
        {
            foreach (
                AccommodationUnitAmenity unitAmenity
                in unit.AccommodationUnitAmenities)
            {
                amenityIds.Add(
                    unitAmenity.AmenityId);
            }
        }

        return amenityIds;
    }

    private static bool CanAppearInSearch(
        Accommodation accommodation)
    {
        return
            accommodation.ApprovalStatus ==
                ApprovalStatus.Approved &&
            accommodation.IsActive &&
            accommodation.Owner.IsActive;
    }

    private async Task ValidateAmenitiesAsync(
        HashSet<int> amenityIds)
    {
        if (amenityIds.Count == 0)
        {
            return;
        }

        List<Amenity> amenities =
            await _unitOfWork.Amenities
                .GetByIdsAsync(
                    amenityIds.ToList());

        if (amenities.Count != amenityIds.Count)
        {
            throw new ValidationException(
                "One or more amenities do not exist.");
        }
    }

    private static void ValidateSearch(
        AccommodationSearchDto dto)
    {
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

        DateOnly today =
            DateOnly.FromDateTime(
                DateTime.UtcNow);

        if (dto.CheckIn < today)
        {
            throw new ValidationException(
                "Check-in date cannot be in the past.");
        }

        if (dto.CheckOut <= dto.CheckIn)
        {
            throw new ValidationException(
                "Check-out date must be after check-in date.");
        }

        if (dto.GuestCount <= 0)
        {
            throw new ValidationException(
                "Guest count must be greater than zero.");
        }

        if (dto.MinPrice.HasValue &&
            dto.MinPrice.Value < 0)
        {
            throw new ValidationException(
                "Minimum price cannot be negative.");
        }

        if (dto.MaxPrice.HasValue &&
            dto.MaxPrice.Value < 0)
        {
            throw new ValidationException(
                "Maximum price cannot be negative.");
        }

        if (dto.MinPrice.HasValue &&
            dto.MaxPrice.HasValue &&
            dto.MinPrice.Value > dto.MaxPrice.Value)
        {
            throw new ValidationException(
                "Minimum price cannot be greater than maximum price.");
        }

        if (dto.Type.HasValue &&
            !Enum.IsDefined(
                typeof(AccommodationType),
                dto.Type.Value))
        {
            throw new ValidationException(
                "Invalid accommodation type.");
        }

        if (!Enum.IsDefined(
                typeof(AccommodationSearchSort),
                dto.Sort))
        {
            throw new ValidationException(
                "Invalid sort option.");
        }
    }

    private static List<AccommodationSearchResultDto>
        SortResults(
            List<AccommodationSearchResultDto> results,
            AccommodationSearchSort sort)
    {
        return sort switch
        {
            AccommodationSearchSort.PriceAscending =>
                results
                    .OrderBy(r =>
                        r.FromPricePerNight)
                    .ToList(),

            AccommodationSearchSort.PriceDescending =>
                results
                    .OrderByDescending(r =>
                        r.FromPricePerNight)
                    .ToList(),

            AccommodationSearchSort.NameAscending =>
                results
                    .OrderBy(r => r.Name)
                    .ToList(),

            AccommodationSearchSort.NameDescending =>
                results
                    .OrderByDescending(r =>
                        r.Name)
                    .ToList(),

            _ => results
        };
    }
}