using HotelReservation.Application.DTOs.Review;

namespace HotelReservation.Application.Interfaces.Services;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(
        int accountId,
        CreateReviewDto dto);

    Task<ReviewDto> UpdateAsync(
        int accountId,
        int reviewId,
        UpdateReviewDto dto);

    Task DeleteOwnAsync(
        int accountId,
        int reviewId);

    Task DeleteByAdminAsync(
        int adminId,
        int reviewId);

    Task<AccommodationReviewsDto>
        GetByAccommodationAsync(
            int accommodationId);
}