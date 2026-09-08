using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces.Repositories;

public interface IAccommodationRepository
{
    Task<Accommodation?> GetByIdAsync(int id);

    Task<Accommodation?> GetByIdWithDetailsAsync(int id);

    Task<List<Accommodation>> GetByOwnerIdAsync(int ownerId);

    Task<List<Accommodation>> GetAllAsync();

    Task AddAsync(Accommodation accommodation);

    void Remove(Accommodation accommodation);
}