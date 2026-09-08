using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HotelReservation.Domain.Entities;

namespace HotelReservation.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);

    Task<Account?> GetByEmailAsync(string email);

    Task<List<Account>> GetAllAsync();

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(Account account);
}