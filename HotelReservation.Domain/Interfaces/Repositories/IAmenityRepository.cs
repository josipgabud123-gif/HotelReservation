using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces.Repositories;

public interface IAmenityRepository
{
    Task<Amenity?> GetByIdAsync(int id);

    Task<List<Amenity>> GetAllAsync();

    Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids);
}