using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationDto> CreateAsync(
        int accountId,
        CreateReservationDto dto)
    {
        ValidateReservationData(dto);

        return await _unitOfWork
            .ExecuteInTransactionAsync(
                async () =>
                {
                    Account? account =
                        await _unitOfWork.Accounts
                            .GetByIdAsync(accountId);

                    if (account is null)
                    {
                        throw new NotFoundException(
                            "Account was not found.");
                    }

                    if (!account.IsActive)
                    {
                        throw new ForbiddenException(
                            "Inactive account cannot create reservations.");
                    }

                    if (account is Admin)
                    {
                        throw new ForbiddenException(
                            "Admin cannot create reservations.");
                    }

                    AccommodationUnit? unit =
                        await _unitOfWork.AccommodationUnits
                            .GetByIdWithDetailsAsync(
                                dto.AccommodationUnitId);

                    if (unit is null)
                    {
                        throw new NotFoundException(
                            "Accommodation unit was not found.");
                    }

                    Accommodation accommodation =
                        unit.Accommodation;

                    if (accommodation.ApprovalStatus !=
                        ApprovalStatus.Approved)
                    {
                        throw new ConflictException(
                            "Accommodation is not approved.");
                    }

                    if (!accommodation.IsActive)
                    {
                        throw new ConflictException(
                            "Accommodation is not active.");
                    }

                    if (!accommodation.Owner.IsActive)
                    {
                        throw new ConflictException(
                            "Accommodation owner is inactive.");
                    }

                    if (!unit.IsActive)
                    {
                        throw new ConflictException(
                            "Accommodation unit is not active.");
                    }

                    if (account is Owner &&
                        accommodation.OwnerId == accountId)
                    {
                        throw new ConflictException(
                            "Owner cannot reserve their own accommodation.");
                    }

                    if (unit.Capacity < dto.GuestCount)
                    {
                        throw new ConflictException(
                            "Accommodation unit does not have enough capacity.");
                    }

                    bool reservationOverlap =
                        await _unitOfWork.Reservations
                            .HasOverlappingConfirmedReservationAsync(
                                unit.Id,
                                dto.CheckInDate,
                                dto.CheckOutDate);

                    if (reservationOverlap)
                    {
                        throw new ConflictException(
                            "Accommodation unit is already reserved for the selected dates.");
                    }

                    bool blocked =
                        await _unitOfWork.AvailabilityBlocks
                            .IsUnitBlockedAsync(
                                accommodation.Id,
                                unit.Id,
                                dto.CheckInDate,
                                dto.CheckOutDate);

                    if (blocked)
                    {
                        throw new ConflictException(
                            "Accommodation unit is unavailable for the selected dates.");
                    }

                    int nights =
                        dto.CheckOutDate.DayNumber -
                        dto.CheckInDate.DayNumber;

                    decimal totalPrice =
                        unit.PricePerNight *
                        nights;

                    var reservation =
                        new Domain.Entities.Reservation
                        {
                            CheckInDate =
                                dto.CheckInDate,

                            CheckOutDate =
                                dto.CheckOutDate,

                            GuestCount =
                                dto.GuestCount,

                            TotalPrice =
                                totalPrice,

                            CancellationPolicyAtBooking =
                                unit.CancellationPolicy,

                            Status =
                                ReservationStatus.Confirmed,

                            CreatedAt =
                                DateTime.UtcNow,

                            CancelledAt =
                                null,

                            AccountId =
                                accountId,

                            AccommodationUnitId =
                                unit.Id,

                            CancelledByAccountId =
                                null
                        };

                    await _unitOfWork.Reservations
                        .AddAsync(reservation);

                    await _unitOfWork
                        .SaveChangesAsync();

                    reservation.Account =
                        account;

                    reservation.AccommodationUnit =
                        unit;

                    return Map(reservation);
                });
    }

    public async Task<List<ReservationDto>>
        GetMyReservationsAsync(
            int accountId)
    {
        List<Domain.Entities.Reservation> reservations =
            await _unitOfWork.Reservations
                .GetByAccountIdAsync(
                    accountId);

        return reservations
            .OrderByDescending(r =>
                r.CreatedAt)
            .Select(Map)
            .ToList();
    }

    public async Task<List<ReservationDto>>
        GetOwnerReservationsAsync(
            int ownerId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(ownerId);

        if (account is not Owner)
        {
            throw new ForbiddenException(
                "Only Owner accounts can access owner reservations.");
        }

        List<Domain.Entities.Reservation> reservations =
            await _unitOfWork.Reservations
                .GetByOwnerIdAsync(
                    ownerId);

        return reservations
            .OrderByDescending(r =>
                r.CreatedAt)
            .Select(Map)
            .ToList();
    }

    public async Task<List<ReservationDto>>
        GetAllAsync()
    {
        List<Domain.Entities.Reservation> reservations =
            await _unitOfWork.Reservations
                .GetAllAsync();

        return reservations
            .OrderByDescending(r =>
                r.CreatedAt)
            .Select(Map)
            .ToList();
    }

    public async Task CancelByGuestAsync(
        int accountId,
        int reservationId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(accountId);

        if (account is null ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Inactive account cannot cancel reservations.");
        }

        Domain.Entities.Reservation reservation =
            await GetReservationAsync(
                reservationId);

        if (reservation.AccountId != accountId)
        {
            throw new ForbiddenException(
                "You cannot cancel another user's reservation.");
        }

        ValidateCanBeCancelled(
            reservation);

        int requiredDays =
            GetRequiredCancellationDays(
                reservation
                    .CancellationPolicyAtBooking);

        DateOnly today = Today();

        int daysUntilCheckIn =
            reservation.CheckInDate.DayNumber -
            today.DayNumber;

        if (daysUntilCheckIn < requiredDays)
        {
            throw new ConflictException(
                $"Reservation must be cancelled at least {requiredDays} day(s) before check-in.");
        }

        CancelReservation(
            reservation,
            accountId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelByOwnerAsync(
        int ownerId,
        int reservationId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(ownerId);

        if (account is not Owner ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Only an active Owner can use this operation.");
        }

        Domain.Entities.Reservation reservation =
            await GetReservationAsync(
                reservationId);

        if (reservation
                .AccommodationUnit
                .Accommodation
                .OwnerId != ownerId)
        {
            throw new ForbiddenException(
                "You do not own the accommodation for this reservation.");
        }

        ValidateCanBeCancelled(
            reservation);

        CancelReservation(
            reservation,
            ownerId);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelByAdminAsync(
        int adminId,
        int reservationId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(adminId);

        if (account is not Admin ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Only an active Admin can use this operation.");
        }

        Domain.Entities.Reservation reservation =
            await GetReservationAsync(
                reservationId);

        ValidateCanBeCancelled(
            reservation);

        CancelReservation(
            reservation,
            adminId);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Domain.Entities.Reservation>
        GetReservationAsync(
            int reservationId)
    {
        Domain.Entities.Reservation? reservation =
            await _unitOfWork.Reservations
                .GetByIdWithDetailsAsync(
                    reservationId);

        if (reservation is null)
        {
            throw new NotFoundException(
                "Reservation was not found.");
        }

        return reservation;
    }

    private static void ValidateReservationData(
        CreateReservationDto dto)
    {
        DateOnly today = Today();

        if (dto.CheckInDate < today)
        {
            throw new ValidationException(
                "Check-in date cannot be in the past.");
        }

        if (dto.CheckOutDate <=
            dto.CheckInDate)
        {
            throw new ValidationException(
                "Check-out date must be after check-in date.");
        }

        if (dto.GuestCount <= 0)
        {
            throw new ValidationException(
                "Guest count must be greater than zero.");
        }
    }

    private static void ValidateCanBeCancelled(
        Domain.Entities.Reservation reservation)
    {
        if (reservation.Status !=
            ReservationStatus.Confirmed)
        {
            throw new ConflictException(
                "Only confirmed reservations can be cancelled.");
        }

        DateOnly today = Today();

        if (today >= reservation.CheckInDate)
        {
            throw new ConflictException(
                "Reservation cannot be cancelled on or after the check-in date.");
        }
    }

    private static int GetRequiredCancellationDays(
        CancellationPolicy policy)
    {
        return policy switch
        {
            CancellationPolicy.Flexible => 1,
            CancellationPolicy.Moderate => 3,
            CancellationPolicy.Strict => 7,

            _ => throw new ValidationException(
                "Invalid cancellation policy.")
        };
    }

    private static void CancelReservation(
        Domain.Entities.Reservation reservation,
        int cancelledByAccountId)
    {
        reservation.Status =
            ReservationStatus.Cancelled;

        reservation.CancelledAt =
            DateTime.UtcNow;

        reservation.CancelledByAccountId =
            cancelledByAccountId;
    }

    private static DateOnly Today()
    {
        return DateOnly.FromDateTime(
            DateTime.Today);
    }

    private static ReservationDto Map(
        Domain.Entities.Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.Id,

            CheckInDate =
                reservation.CheckInDate,

            CheckOutDate =
                reservation.CheckOutDate,

            GuestCount =
                reservation.GuestCount,

            TotalPrice =
                reservation.TotalPrice,

            CancellationPolicyAtBooking =
                reservation
                    .CancellationPolicyAtBooking,

            Status =
                reservation.Status,

            CreatedAt =
                reservation.CreatedAt,

            CancelledAt =
                reservation.CancelledAt,

            AccountId =
                reservation.AccountId,

            AccommodationUnitId =
                reservation.AccommodationUnitId,

            UnitName =
                reservation
                    .AccommodationUnit
                    .Name,

            AccommodationId =
                reservation
                    .AccommodationUnit
                    .Accommodation
                    .Id,

            AccommodationName =
                reservation
                    .AccommodationUnit
                    .Accommodation
                    .Name,

            CancelledByAccountId =
                reservation
                    .CancelledByAccountId
        };
    }
}