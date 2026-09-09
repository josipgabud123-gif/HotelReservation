using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);

    Task<Reservation?> GetByIdWithDetailsAsync(int id);

    Task<List<Reservation>> GetByAccountIdAsync(
        int accountId);

    Task<List<Reservation>> GetByOwnerIdAsync(
        int ownerId);

    Task<List<Reservation>> GetAllAsync();

    Task<bool> HasOverlappingConfirmedReservationAsync(
        int accommodationUnitId,
        DateOnly checkInDate,
        DateOnly checkOutDate);

    Task<bool> UnitHasAnyReservationAsync(
        int accommodationUnitId);

    Task<List<Reservation>>
        GetConfirmedReservationsForCompletionAsync(
            DateOnly today);

    Task AddAsync(Reservation reservation);
}