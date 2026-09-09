using HotelReservation.Application.DTOs.AvailabilityBlock;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AvailabilityBlockService
    : IAvailabilityBlockService
{
    private readonly IUnitOfWork _unitOfWork;

    public AvailabilityBlockService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AvailabilityBlockDto>
        CreateForAccommodationAsync(
            int ownerId,
            int accommodationId,
            CreateAvailabilityBlockDto dto)
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

        EnsureOwnerCanManage(
            accommodation,
            ownerId);

        ValidateDates(
            dto.StartDate,
            dto.EndDate);

        bool overlaps =
            await _unitOfWork.AvailabilityBlocks
                .HasOverlappingBlockAsync(
                    accommodationId,
                    null,
                    dto.StartDate,
                    dto.EndDate);

        if (overlaps)
        {
            throw new ConflictException(
                "An overlapping availability block already exists.");
        }

        var block = new AvailabilityBlock
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AccommodationId = accommodationId,
            AccommodationUnitId = null,
            CreatedByOwnerId = ownerId
        };

        await _unitOfWork.AvailabilityBlocks
            .AddAsync(block);

        await _unitOfWork.SaveChangesAsync();

        return Map(block);
    }

    public async Task<AvailabilityBlockDto>
        CreateForUnitAsync(
            int ownerId,
            int accommodationId,
            int unitId,
            CreateAvailabilityBlockDto dto)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null ||
            unit.AccommodationId != accommodationId)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        EnsureOwnerCanManage(
            unit.Accommodation,
            ownerId);

        if (unit.Accommodation.Type !=
            AccommodationType.Hotel)
        {
            throw new ConflictException(
                "Apartment unit cannot be blocked separately.");
        }

        ValidateDates(
            dto.StartDate,
            dto.EndDate);

        bool overlaps =
            await _unitOfWork.AvailabilityBlocks
                .HasOverlappingBlockAsync(
                    null,
                    unitId,
                    dto.StartDate,
                    dto.EndDate);

        if (overlaps)
        {
            throw new ConflictException(
                "An overlapping availability block already exists for this unit.");
        }

        var block = new AvailabilityBlock
        {
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AccommodationId = null,
            AccommodationUnitId = unitId,
            CreatedByOwnerId = ownerId
        };

        await _unitOfWork.AvailabilityBlocks
            .AddAsync(block);

        await _unitOfWork.SaveChangesAsync();

        return Map(block);
    }

    public async Task<List<AvailabilityBlockDto>>
        GetAccommodationBlocksAsync(
            int ownerId,
            int accommodationId,
            bool includePast)
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

        if (accommodation.OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        List<AvailabilityBlock> blocks =
            await _unitOfWork.AvailabilityBlocks
                .GetByAccommodationIdAsync(
                    accommodationId);

        DateOnly today = Today();

        if (!includePast)
        {
            blocks = blocks
                .Where(b => b.EndDate > today)
                .ToList();
        }

        return blocks
            .OrderBy(b => b.StartDate)
            .Select(Map)
            .ToList();
    }

    public async Task<List<AvailabilityBlockDto>>
        GetUnitBlocksAsync(
            int ownerId,
            int accommodationId,
            int unitId,
            bool includePast)
    {
        AccommodationUnit? unit =
            await _unitOfWork.AccommodationUnits
                .GetByIdWithDetailsAsync(unitId);

        if (unit is null ||
            unit.AccommodationId != accommodationId)
        {
            throw new NotFoundException(
                "Accommodation unit was not found.");
        }

        if (unit.Accommodation.OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own this accommodation.");
        }

        if (unit.Accommodation.Type !=
            AccommodationType.Hotel)
        {
            throw new ConflictException(
                "Apartment unit does not have separate availability blocks.");
        }

        List<AvailabilityBlock> blocks =
            await _unitOfWork.AvailabilityBlocks
                .GetByAccommodationUnitIdAsync(
                    unitId);

        DateOnly today = Today();

        if (!includePast)
        {
            blocks = blocks
                .Where(b => b.EndDate > today)
                .ToList();
        }

        return blocks
            .OrderBy(b => b.StartDate)
            .Select(Map)
            .ToList();
    }

    public async Task<AvailabilityBlockDto>
        UpdateAsync(
            int ownerId,
            int blockId,
            UpdateAvailabilityBlockDto dto)
    {
        AvailabilityBlock? block =
            await _unitOfWork.AvailabilityBlocks
                .GetByIdAsync(blockId);

        if (block is null)
        {
            throw new NotFoundException(
                "Availability block was not found.");
        }

        if (block.CreatedByOwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You cannot modify this availability block.");
        }

        Accommodation accommodation =
            await GetParentAccommodationAsync(block);

        EnsureOwnerCanManage(
            accommodation,
            ownerId);

        DateOnly today = Today();

        if (block.EndDate <= today)
        {
            throw new ConflictException(
                "Past availability blocks cannot be edited.");
        }

        if (dto.EndDate <= dto.StartDate)
        {
            throw new ValidationException(
                "End date must be after start date.");
        }

        if (block.StartDate <= today)
        {
            if (dto.StartDate != block.StartDate)
            {
                throw new ConflictException(
                    "Start date cannot be changed after the block has started.");
            }
        }
        else if (dto.StartDate < today)
        {
            throw new ValidationException(
                "Start date cannot be in the past.");
        }

        bool overlaps =
            await _unitOfWork.AvailabilityBlocks
                .HasOverlappingBlockAsync(
                    block.AccommodationId,
                    block.AccommodationUnitId,
                    dto.StartDate,
                    dto.EndDate,
                    block.Id);

        if (overlaps)
        {
            throw new ConflictException(
                "An overlapping availability block already exists.");
        }

        block.StartDate = dto.StartDate;
        block.EndDate = dto.EndDate;

        await _unitOfWork.SaveChangesAsync();

        return Map(block);
    }

    public async Task DeleteAsync(
        int ownerId,
        int blockId)
    {
        AvailabilityBlock? block =
            await _unitOfWork.AvailabilityBlocks
                .GetByIdAsync(blockId);

        if (block is null)
        {
            throw new NotFoundException(
                "Availability block was not found.");
        }

        if (block.CreatedByOwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You cannot delete this availability block.");
        }

        Accommodation accommodation =
            await GetParentAccommodationAsync(block);

        EnsureOwnerCanManage(
            accommodation,
            ownerId);

        DateOnly today = Today();

        if (block.EndDate <= today)
        {
            throw new ConflictException(
                "Past availability blocks cannot be deleted.");
        }

        _unitOfWork.AvailabilityBlocks
            .Remove(block);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Accommodation>
        GetParentAccommodationAsync(
            AvailabilityBlock block)
    {
        if (block.AccommodationId.HasValue)
        {
            Accommodation? accommodation =
                await _unitOfWork.Accommodations
                    .GetByIdWithDetailsAsync(
                        block.AccommodationId.Value);

            if (accommodation is null)
            {
                throw new NotFoundException(
                    "Accommodation was not found.");
            }

            return accommodation;
        }

        if (block.AccommodationUnitId.HasValue)
        {
            AccommodationUnit? unit =
                await _unitOfWork.AccommodationUnits
                    .GetByIdWithDetailsAsync(
                        block.AccommodationUnitId.Value);

            if (unit is null)
            {
                throw new NotFoundException(
                    "Accommodation unit was not found.");
            }

            return unit.Accommodation;
        }

        throw new NotFoundException(
            "Availability block target was not found.");
    }

    private static void EnsureOwnerCanManage(
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
                "Inactive Owner cannot manage availability.");
        }

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

    private static void ValidateDates(
        DateOnly startDate,
        DateOnly endDate)
    {
        DateOnly today = Today();

        if (startDate < today)
        {
            throw new ValidationException(
                "Start date cannot be in the past.");
        }

        if (endDate <= startDate)
        {
            throw new ValidationException(
                "End date must be after start date.");
        }
    }

    private static DateOnly Today()
    {
        return DateOnly.FromDateTime(
            DateTime.UtcNow);
    }

    private static AvailabilityBlockDto Map(
        AvailabilityBlock block)
    {
        return new AvailabilityBlockDto
        {
            Id = block.Id,
            StartDate = block.StartDate,
            EndDate = block.EndDate,
            AccommodationId =
                block.AccommodationId,
            AccommodationUnitId =
                block.AccommodationUnitId,
            CreatedByOwnerId =
                block.CreatedByOwnerId
        };
    }
}