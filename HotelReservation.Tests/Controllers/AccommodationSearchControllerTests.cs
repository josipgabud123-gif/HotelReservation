using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Search;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AccommodationSearchControllerTests
{
    private readonly Mock<IAccommodationSearchService>
        _serviceMock;

    private readonly AccommodationSearchController
        _controller;

    public AccommodationSearchControllerTests()
    {
        _serviceMock =
            new Mock<IAccommodationSearchService>();

        _controller =
            new AccommodationSearchController(
                _serviceMock.Object);
    }

    [Fact]
    public async Task Search_ShouldReturnOk_WithResults()
    {
        // Arrange
        var dto =
            new AccommodationSearchDto
            {
                City = "Split",
                Country = "Croatia",
                CheckIn =
                    new DateOnly(
                        2026,
                        11,
                        10),
                CheckOut =
                    new DateOnly(
                        2026,
                        11,
                        15),
                GuestCount = 2
            };

        var expected =
            new List<AccommodationSearchResultDto>
            {
                new()
                {
                    Id = 1,
                    Name = "Hotel Test",
                    City = "Split",
                    Country = "Croatia",
                    FromPricePerNight = 100,
                    FromTotalPrice = 500
                }
            };

        _serviceMock
            .Setup(s =>
                s.SearchAsync(dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<
            List<AccommodationSearchResultDto>>
            result =
                await _controller.Search(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        List<AccommodationSearchResultDto>
            response =
                Assert.IsType<
                    List<AccommodationSearchResultDto>>(
                    okResult.Value);

        Assert.Single(response);

        Assert.Equal(
            "Hotel Test",
            response[0].Name);

        Assert.Equal(
            500,
            response[0].FromTotalPrice);

        _serviceMock.Verify(
            s => s.SearchAsync(dto),
            Times.Once);
    }

    [Fact]
    public async Task Search_WhenNoAccommodationFound_ShouldReturnEmptyList()
    {
        // Arrange
        var dto =
            new AccommodationSearchDto
            {
                City = "Zagreb",
                Country = "Croatia",
                CheckIn =
                    new DateOnly(
                        2026,
                        12,
                        1),
                CheckOut =
                    new DateOnly(
                        2026,
                        12,
                        5),
                GuestCount = 2
            };

        _serviceMock
            .Setup(s =>
                s.SearchAsync(dto))
            .ReturnsAsync(
                new List<
                    AccommodationSearchResultDto>());

        // Act
        ActionResult<
            List<AccommodationSearchResultDto>>
            result =
                await _controller.Search(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        List<AccommodationSearchResultDto>
            response =
                Assert.IsType<
                    List<AccommodationSearchResultDto>>(
                    okResult.Value);

        Assert.Empty(response);

        _serviceMock.Verify(
            s => s.SearchAsync(dto),
            Times.Once);
    }
}