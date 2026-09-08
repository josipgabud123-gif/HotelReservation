using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelReservation.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly HotelReservationDbContext _context;

    public AccountRepository(HotelReservationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Email == email);
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }
}