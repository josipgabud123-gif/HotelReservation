using HotelReservation.Application.DTOs.Account;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AccountManagementService
    : IAccountManagementService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountManagementService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AccountDto>> GetAllAsync()
    {
        List<Account> accounts =
            await _unitOfWork.Accounts.GetAllAsync();

        return accounts
            .OrderBy(a => a.Id)
            .Select(Map)
            .ToList();
    }

    public async Task<AccountDto> SetActiveStatusAsync(
        int adminId,
        int accountId,
        bool isActive)
    {
        Account? admin =
            await _unitOfWork.Accounts
                .GetByIdAsync(adminId);

        if (admin is not Admin ||
            !admin.IsActive)
        {
            throw new ForbiddenException(
                "Only an active Admin can manage accounts.");
        }

        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(accountId);

        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        if (adminId == accountId &&
            !isActive)
        {
            throw new ConflictException(
                "Admin cannot deactivate their own account.");
        }

        account.IsActive = isActive;

        await _unitOfWork.SaveChangesAsync();

        return Map(account);
    }

    private static AccountDto Map(
        Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Role = account.Role,
            IsActive = account.IsActive
        };
    }
}