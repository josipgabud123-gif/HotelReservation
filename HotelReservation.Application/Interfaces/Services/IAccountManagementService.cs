using HotelReservation.Application.DTOs.Account;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAccountManagementService
{
    Task<List<AccountDto>> GetAllAsync();

    Task<AccountDto> SetActiveStatusAsync(
        int adminId,
        int accountId,
        bool isActive);
}