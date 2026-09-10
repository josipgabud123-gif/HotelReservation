using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class ReservationsControllerTests
{
    private const int AccountId = 3;

    private readonly Mock<IReservationService> _serviceMock;
    private readonly ReservationsController _controller;

    public ReservationsControllerTests()
    {
        _serviceMock = new Mock<IReservationService>();
        _controller = new ReservationsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            AccountId,
            "Customer");
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WithReservation()
    {
        // Arrange
        var dto = new CreateReservationDto
        {
            AccommodationUnitId = 2,
            CheckInDate = new DateOnly(2026, 11, 10),
            CheckOutDate = new DateOnly(2026, 11, 15),
            GuestCount = 2
        };

        var expected = new ReservationDto
        {
            Id = 1,
            AccountId = AccountId,
            AccommodationUnitId = 2,
            TotalPrice = 850
        };

        _serviceMock
            .Setup(s => s.CreateAsync(
                AccountId,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<ReservationDto> result =
            await _controller.Create(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        ReservationDto response =
            Assert.IsType<ReservationDto>(okResult.Value);

        Assert.Equal(1, response.Id);
        Assert.Equal(850, response.TotalPrice);

        _serviceMock.Verify(
            s => s.CreateAsync(AccountId, dto),
            Times.Once);
    }

    [Fact]
    public async Task GetMyReservations_ShouldReturnOk_WithReservations()
    {
        // Arrange
        var expected = new List<ReservationDto>
        {
            new() { Id = 1, AccountId = AccountId }
        };

        _serviceMock
            .Setup(s => s.GetMyReservationsAsync(AccountId))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<ReservationDto>> result =
            await _controller.GetMyReservations();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<ReservationDto> response =
            Assert.IsType<List<ReservationDto>>(okResult.Value);

        Assert.Single(response);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Cancel(1);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.CancelByGuestAsync(AccountId, 1),
            Times.Once);
    }
}
