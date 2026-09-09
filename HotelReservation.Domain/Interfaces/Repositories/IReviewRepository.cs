using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(
        int id);

    Task<Review?> GetByReservationIdAsync(
        int reservationId);

    Task<List<Review>>
        GetByAccommodationIdAsync(
            int accommodationId);

    Task AddAsync(
        Review review);

    void Remove(
        Review review);
}