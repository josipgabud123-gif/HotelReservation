using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AdminReservationsControllerTests
{
    private const int AdminId = 1;

    private readonly Mock<IReservationService> _serviceMock;
    private readonly AdminReservationsController _controller;

    public AdminReservationsControllerTests()
    {
        _serviceMock = new Mock<IReservationService>();
        _controller = new AdminReservationsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            AdminId,
            "Admin");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithReservations()
    {
        // Arrange
        var expected = new List<ReservationDto>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };

        _serviceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<ReservationDto>> result =
            await _controller.GetAll();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<ReservationDto> response =
            Assert.IsType<List<ReservationDto>>(okResult.Value);

        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task Cancel_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Cancel(2);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.CancelByAdminAsync(AdminId, 2),
            Times.Once);
    }
}
