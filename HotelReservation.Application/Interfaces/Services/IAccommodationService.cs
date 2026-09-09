using HotelReservation.Application.DTOs.Accommodation;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAccommodationService
{
    Task<AccommodationDto> CreateAsync(
        int ownerId,
        CreateAccommodationDto dto);

    Task<List<AccommodationDto>> GetOwnerAccommodationsAsync(
        int ownerId);

    Task<AccommodationDto> UpdateAsync(
        int ownerId,
        int accommodationId,
        UpdateAccommodationDto dto);

    Task SubmitForApprovalAsync(
        int ownerId,
        int accommodationId);

    Task ActivateAsync(
        int ownerId,
        int accommodationId);

    Task DeactivateAsync(
        int ownerId,
        int accommodationId);

    Task DeleteAsync(
        int ownerId,
        int accommodationId);

    Task<List<AccommodationDto>> GetPendingAsync();

    Task ApproveAsync(int accommodationId);

    Task RejectAsync(
        int accommodationId,
        string reason);

    Task SuspendAsync(int accommodationId);

    Task UnsuspendAsync(int accommodationId);

    Task<AccommodationPhotoDto> AddPhotoAsync(
    int ownerId,
    int accommodationId,
    Stream fileStream,
    string fileName,
    bool isPrimary);

    Task DeletePhotoAsync(
        int ownerId,
        int accommodationId,
        int photoId);

    Task SetPrimaryPhotoAsync(
        int ownerId,
        int accommodationId,
        int photoId);
}