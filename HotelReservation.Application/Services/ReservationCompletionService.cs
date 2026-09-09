using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class ReservationCompletionService
    : IReservationCompletionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReservationCompletionService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CompleteExpiredReservationsAsync()
    {
        DateOnly today =
            DateOnly.FromDateTime(DateTime.Today);

        List<Reservation> reservations =
            await _unitOfWork.Reservations
                .GetConfirmedReservationsForCompletionAsync(
                    today);

        if (reservations.Count == 0)
        {
            return 0;
        }

        foreach (Reservation reservation in reservations)
        {
            reservation.Status =
                ReservationStatus.Completed;
        }

        await _unitOfWork.SaveChangesAsync();

        return reservations.Count;
    }
}