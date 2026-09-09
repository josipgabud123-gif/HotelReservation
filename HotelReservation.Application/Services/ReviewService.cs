using HotelReservation.Application.DTOs.Review;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Interfaces;

namespace HotelReservation.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReviewDto> CreateAsync(
        int accountId,
        CreateReviewDto dto)
    {
        ValidateReview(
            dto.Rating,
            dto.Comment);

        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(accountId);

        if (account is null)
        {
            throw new NotFoundException(
                "Account was not found.");
        }

        if (!account.IsActive)
        {
            throw new ForbiddenException(
                "Inactive account cannot create reviews.");
        }

        if (account is Admin)
        {
            throw new ForbiddenException(
                "Admin cannot create reviews.");
        }

        Reservation? reservation =
            await _unitOfWork.Reservations
                .GetByIdWithDetailsAsync(
                    dto.ReservationId);

        if (reservation is null)
        {
            throw new NotFoundException(
                "Reservation was not found.");
        }

        if (reservation.AccountId !=
            accountId)
        {
            throw new ForbiddenException(
                "You can only review your own reservation.");
        }

        if (reservation.Status !=
            ReservationStatus.Completed)
        {
            throw new ConflictException(
                "Review can only be created after the reservation is completed.");
        }

        Review? existingReview =
            await _unitOfWork.Reviews
                .GetByReservationIdAsync(
                    reservation.Id);

        if (existingReview is not null)
        {
            throw new ConflictException(
                "This reservation already has a review.");
        }

        int accommodationId =
            reservation.AccommodationUnit
                .Accommodation.Id;

        var review = new Review
        {
            Rating = dto.Rating,

            Comment =
                NormalizeComment(
                    dto.Comment),

            CreatedAt =
                DateTime.UtcNow,

            AccountId =
                accountId,

            AccommodationId =
                accommodationId,

            ReservationId =
                reservation.Id
        };

        await _unitOfWork.Reviews
            .AddAsync(review);

        await _unitOfWork.SaveChangesAsync();

        review.Account = account;

        return Map(review);
    }

    public async Task<ReviewDto> UpdateAsync(
        int accountId,
        int reviewId,
        UpdateReviewDto dto)
    {
        ValidateReview(
            dto.Rating,
            dto.Comment);

        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(accountId);

        if (account is null ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Inactive account cannot edit reviews.");
        }

        Review? review =
            await _unitOfWork.Reviews
                .GetByIdAsync(
                    reviewId);

        if (review is null)
        {
            throw new NotFoundException(
                "Review was not found.");
        }

        if (review.AccountId !=
            accountId)
        {
            throw new ForbiddenException(
                "You cannot edit another user's review.");
        }

        review.Rating =
            dto.Rating;

        review.Comment =
            NormalizeComment(
                dto.Comment);

        await _unitOfWork.SaveChangesAsync();

        return Map(review);
    }

    public async Task DeleteOwnAsync(
        int accountId,
        int reviewId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(accountId);

        if (account is null ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Inactive account cannot delete reviews.");
        }

        Review? review =
            await _unitOfWork.Reviews
                .GetByIdAsync(
                    reviewId);

        if (review is null)
        {
            throw new NotFoundException(
                "Review was not found.");
        }

        if (review.AccountId !=
            accountId)
        {
            throw new ForbiddenException(
                "You cannot delete another user's review.");
        }

        _unitOfWork.Reviews
            .Remove(review);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteByAdminAsync(
        int adminId,
        int reviewId)
    {
        Account? account =
            await _unitOfWork.Accounts
                .GetByIdAsync(adminId);

        if (account is not Admin ||
            !account.IsActive)
        {
            throw new ForbiddenException(
                "Only an active Admin can delete any review.");
        }

        Review? review =
            await _unitOfWork.Reviews
                .GetByIdAsync(
                    reviewId);

        if (review is null)
        {
            throw new NotFoundException(
                "Review was not found.");
        }

        _unitOfWork.Reviews
            .Remove(review);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<AccommodationReviewsDto>
        GetByAccommodationAsync(
            int accommodationId)
    {
        Accommodation? accommodation =
            await _unitOfWork.Accommodations
                .GetByIdAsync(
                    accommodationId);

        if (accommodation is null)
        {
            throw new NotFoundException(
                "Accommodation was not found.");
        }

        List<Review> reviews =
            await _unitOfWork.Reviews
                .GetByAccommodationIdAsync(
                    accommodationId);

        double? averageRating =
            reviews.Count == 0
                ? null
                : reviews.Average(r =>
                    (double)r.Rating);

        return new AccommodationReviewsDto
        {
            AccommodationId =
                accommodationId,

            AverageRating =
                averageRating,

            ReviewCount =
                reviews.Count,

            Reviews =
                reviews
                    .Select(Map)
                    .ToList()
        };
    }

    private static void ValidateReview(
        int rating,
        string? comment)
    {
        if (rating < 1 ||
            rating > 5)
        {
            throw new ValidationException(
                "Rating must be between 1 and 5.");
        }

        if (comment is not null &&
            comment.Trim().Length > 2000)
        {
            throw new ValidationException(
                "Review comment cannot exceed 2000 characters.");
        }
    }

    private static string? NormalizeComment(
        string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return null;
        }

        return comment.Trim();
    }

    private static ReviewDto Map(
        Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            AccountId = review.AccountId,

            ReviewerName =
                $"{review.Account.FirstName} {review.Account.LastName}",

            AccommodationId =
                review.AccommodationId,

            ReservationId =
                review.ReservationId
        };
    }
}