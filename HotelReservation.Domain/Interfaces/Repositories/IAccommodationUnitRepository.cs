using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces;

public interface IAccommodationUnitRepository
{
    Task<AccommodationUnit?> GetByIdAsync(int id);

    Task<AccommodationUnit?> GetByIdWithAccommodationAsync(
        int id);

    Task<AccommodationUnit?> GetByIdWithDetailsAsync(
        int id);

    Task<List<AccommodationUnit>> GetByAccommodationIdAsync(
        int accommodationId);

    Task<bool> NameExistsAsync(
        int accommodationId,
        string name);

    Task AddAsync(AccommodationUnit unit);

    void Remove(AccommodationUnit unit);

    void RemoveAmenity(
        AccommodationUnitAmenity accommodationUnitAmenity);
}