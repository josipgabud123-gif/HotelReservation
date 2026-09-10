using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class OwnerReservationsControllerTests
{
    private const int OwnerId = 7;

    private readonly Mock<IReservationService> _serviceMock;
    private readonly OwnerReservationsController _controller;

    public OwnerReservationsControllerTests()
    {
        _serviceMock = new Mock<IReservationService>();
        _controller = new OwnerReservationsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            OwnerId,
            "Owner");
    }

    [Fact]
    public async Task GetReservations_ShouldReturnOk_WithOwnerReservations()
    {
        // Arrange
        var expected = new List<ReservationDto>
        {
            new() { Id = 1, AccommodationId = 10 }
        };

        _serviceMock
            .Setup(s => s.GetOwnerReservationsAsync(OwnerId))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<ReservationDto>> result =
            await _controller.GetReservations();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<ReservationDto> response =
            Assert.IsType<List<ReservationDto>>(okResult.Value);

        Assert.Single(response);

        _serviceMock.Verify(
            s => s.GetOwnerReservationsAsync(OwnerId),
            Times.Once);
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
            s => s.CancelByOwnerAsync(OwnerId, 1),
            Times.Once);
    }
}
