using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Review;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class ReviewsControllerTests
{
    private const int AccountId = 3;

    private readonly Mock<IReviewService> _serviceMock;
    private readonly ReviewsController _controller;

    public ReviewsControllerTests()
    {
        _serviceMock = new Mock<IReviewService>();
        _controller = new ReviewsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            AccountId,
            "Customer");
    }

    [Fact]
    public async Task GetByAccommodation_ShouldReturnOk_WithReviews()
    {
        // Arrange
        var expected = new AccommodationReviewsDto
        {
            AccommodationId = 10,
            AverageRating = 4.5,
            ReviewCount = 2,
            Reviews = new List<ReviewDto>
            {
                new() { Id = 1, Rating = 5 },
                new() { Id = 2, Rating = 4 }
            }
        };

        _serviceMock
            .Setup(s => s.GetByAccommodationAsync(10))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationReviewsDto> result =
            await _controller.GetByAccommodation(10);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationReviewsDto response =
            Assert.IsType<AccommodationReviewsDto>(okResult.Value);

        Assert.Equal(2, response.ReviewCount);
        Assert.Equal(4.5, response.AverageRating);
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WithCreatedReview()
    {
        // Arrange
        var dto = new CreateReviewDto
        {
            ReservationId = 1,
            Rating = 5,
            Comment = "Odlican hotel"
        };

        var expected = new ReviewDto
        {
            Id = 4,
            Rating = 5,
            AccountId = AccountId,
            ReservationId = 1
        };

        _serviceMock
            .Setup(s => s.CreateAsync(
                AccountId,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<ReviewDto> result =
            await _controller.Create(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        ReviewDto response =
            Assert.IsType<ReviewDto>(okResult.Value);

        Assert.Equal(4, response.Id);
        Assert.Equal(5, response.Rating);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WithUpdatedReview()
    {
        // Arrange
        var dto = new UpdateReviewDto
        {
            Rating = 4,
            Comment = "Vrlo dobro"
        };

        var expected = new ReviewDto
        {
            Id = 4,
            Rating = 4,
            Comment = "Vrlo dobro",
            AccountId = AccountId
        };

        _serviceMock
            .Setup(s => s.UpdateAsync(
                AccountId,
                4,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<ReviewDto> result =
            await _controller.Update(4, dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        ReviewDto response =
            Assert.IsType<ReviewDto>(okResult.Value);

        Assert.Equal(4, response.Rating);

        _serviceMock.Verify(
            s => s.UpdateAsync(AccountId, 4, dto),
            Times.Once);
    }

    [Fact]
    public async Task DeleteOwn_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.DeleteOwn(4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeleteOwnAsync(AccountId, 4),
            Times.Once);
    }

    [Fact]
    public async Task DeleteByAdmin_ShouldReturnNoContent_AndCallService()
    {
        // Arrange
        ControllerTestHelper.SetUser(
            _controller,
            1,
            "Admin");

        // Act
        IActionResult result =
            await _controller.DeleteByAdmin(4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeleteByAdminAsync(1, 4),
            Times.Once);
    }
}
