using HotelReservation.Application.DTOs.AccommodationUnit;

namespace HotelReservation.Application.Interfaces.Services;

public interface IAccommodationUnitService
{
    Task<AccommodationUnitDto> CreateAsync(
        int ownerId,
        int accommodationId,
        CreateAccommodationUnitDto dto,
        IReadOnlyList<ImageUploadData> photos,
        int primaryPhotoIndex);

    Task<AccommodationUnitDto> UpdateAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        UpdateAccommodationUnitDto dto);

    Task ActivateAsync(
        int ownerId,
        int accommodationId,
        int unitId);

    Task DeactivateAsync(
        int ownerId,
        int accommodationId,
        int unitId);

    Task DeleteAsync(
        int ownerId,
        int accommodationId,
        int unitId);

    Task<AccommodationUnitPhotoDto> AddPhotoAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        Stream fileStream,
        string fileName,
        bool isPrimary);

    Task DeletePhotoAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        int photoId);

    Task SetPrimaryPhotoAsync(
        int ownerId,
        int accommodationId,
        int unitId,
        int photoId);
}
