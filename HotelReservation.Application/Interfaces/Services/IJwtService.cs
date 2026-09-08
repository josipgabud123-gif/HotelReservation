using HotelReservation.Domain.Entities;

namespace HotelReservation.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(Account account);
}