using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces.Repositories;

public interface IAccommodationUnitRepository
{
    Task<AccommodationUnit?> GetByIdAsync(int id);

    Task<AccommodationUnit?> GetByIdWithAccommodationAsync(int id);

    Task<List<AccommodationUnit>> GetByAccommodationIdAsync(int accommodationId);

    Task<bool> NameExistsAsync(int accommodationId, string name);

    Task AddAsync(AccommodationUnit unit);

    void Remove(AccommodationUnit unit);
}