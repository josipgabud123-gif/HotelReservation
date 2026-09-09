using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;

namespace HotelReservation.Domain.Interfaces;

public interface IAccommodationRepository
{
    Task<Accommodation?> GetByIdAsync(int id);

    Task<Accommodation?> GetByIdWithDetailsAsync(int id);

    Task<List<Accommodation>> GetByOwnerIdAsync(int ownerId);

    Task<List<Accommodation>> GetPendingAsync();

    Task<List<Accommodation>> GetAllAsync();

    Task<List<Accommodation>> GetSearchCandidatesAsync(
        string city,
        string country,
        AccommodationType? type,
        string? nameSearch);

    Task<bool> HasAnyReservationAsync(int accommodationId);

    Task AddAsync(Accommodation accommodation);

    void Remove(Accommodation accommodation);

    void RemovePhoto(AccommodationPhoto photo);

    void RemoveAmenity(
        AccommodationAmenity accommodationAmenity);
}