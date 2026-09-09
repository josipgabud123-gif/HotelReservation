namespace HotelReservation.Application.Interfaces.Services;

public interface IReservationCompletionService
{
    Task<int> CompleteExpiredReservationsAsync();
}