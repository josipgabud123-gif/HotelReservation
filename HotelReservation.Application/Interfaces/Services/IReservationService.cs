using HotelReservation.Application.DTOs.Reservation;

namespace HotelReservation.Application.Interfaces.Services;

public interface IReservationService
{
    Task<ReservationDto> CreateAsync(
        int accountId,
        CreateReservationDto dto);

    Task<List<ReservationDto>>
        GetMyReservationsAsync(
            int accountId);

    Task<List<ReservationDto>>
        GetOwnerReservationsAsync(
            int ownerId);

    Task<List<ReservationDto>> GetAllAsync();

    Task CancelByGuestAsync(
        int accountId,
        int reservationId);

    Task CancelByOwnerAsync(
        int ownerId,
        int reservationId);

    Task CancelByAdminAsync(
        int adminId,
        int reservationId);
}