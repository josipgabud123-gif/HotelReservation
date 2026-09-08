using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class AmenityRepository : IAmenityRepository
{
    private readonly HotelReservationDbContext _context;

    public AmenityRepository(HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Amenity?> GetByIdAsync(int id)
    {
        return await _context.Amenities
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Amenity>> GetAllAsync()
    {
        return await _context.Amenities.ToListAsync();
    }

    public async Task<List<Amenity>> GetByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Amenities
            .Where(a => ids.Contains(a.Id))
            .ToListAsync();
    }
}