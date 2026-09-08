using HotelReservation.Application.DTOs.Auth;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            throw new ValidationException(
                "First name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.LastName))
        {
            throw new ValidationException(
                "Last name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException(
                "Email is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) ||
            dto.Password.Length < 8)
        {
            throw new ValidationException(
                "Password must contain at least 8 characters.");
        }

        if (dto.Role != UserRole.Customer &&
            dto.Role != UserRole.Owner)
        {
            throw new ValidationException(
                "Only Customer or Owner accounts can be registered.");
        }

        if (await _unitOfWork.Accounts.EmailExistsAsync(email))
        {
            throw new ConflictException(
                "An account with this email already exists.");
        }

        Account account = dto.Role switch
        {
            UserRole.Customer => new Customer(),
            UserRole.Owner => new Owner(),

            _ => throw new ValidationException(
                "Invalid account role.")
        };

        account.FirstName = dto.FirstName.Trim();
        account.LastName = dto.LastName.Trim();
        account.Email = email;
        account.PasswordHash =
            _passwordHasher.Hash(dto.Password);
        account.IsActive = true;

        await _unitOfWork.Accounts.AddAsync(account);

        await _unitOfWork.SaveChangesAsync();

        string token = _jwtService.GenerateToken(account);

        return CreateResponse(account, token);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        string email = dto.Email.Trim().ToLowerInvariant();

        Account? account =
            await _unitOfWork.Accounts.GetByEmailAsync(email);

        if (account is null)
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        if (!_passwordHasher.Verify(
                dto.Password,
                account.PasswordHash))
        {
            throw new UnauthorizedException(
                "Invalid email or password.");
        }

        if (!account.IsActive)
        {
            throw new UnauthorizedException(
                "This account is inactive.");
        }

        string token = _jwtService.GenerateToken(account);

        return CreateResponse(account, token);
    }

    private static AuthResponseDto CreateResponse(
        Account account,
        string token)
    {
        return new AuthResponseDto
        {
            Id = account.Id,
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Role = account.Role,
            Token = token
        };
    }
}