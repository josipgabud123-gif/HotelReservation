using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Accommodation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AdminAccommodationsControllerTests
{
    private readonly Mock<IAccommodationService> _serviceMock;
    private readonly AdminAccommodationsController _controller;

    public AdminAccommodationsControllerTests()
    {
        _serviceMock = new Mock<IAccommodationService>();
        _controller = new AdminAccommodationsController(
            _serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithAccommodations()
    {
        // Arrange
        var expected = new List<AccommodationDto>
        {
            new()
            {
                Id = 1,
                Name = "Hotel One"
            }
        };

        _serviceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AccommodationDto>> result =
            await _controller.GetAll();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AccommodationDto> response =
            Assert.IsType<List<AccommodationDto>>(
                okResult.Value);

        Assert.Single(response);
        Assert.Equal("Hotel One", response[0].Name);

        _serviceMock.Verify(
            s => s.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetPending_ShouldReturnOk_WithPendingAccommodations()
    {
        // Arrange
        var expected = new List<AccommodationDto>
        {
            new()
            {
                Id = 1,
                Name = "Hotel Pending"
            }
        };

        _serviceMock
            .Setup(s => s.GetPendingAsync())
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AccommodationDto>> result =
            await _controller.GetPending();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AccommodationDto> response =
            Assert.IsType<List<AccommodationDto>>(okResult.Value);

        Assert.Single(response);
        Assert.Equal("Hotel Pending", response[0].Name);

        _serviceMock.Verify(
            s => s.GetPendingAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Approve_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Approve(5);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.ApproveAsync(5),
            Times.Once);
    }

    [Fact]
    public async Task Reject_ShouldReturnNoContent_AndCallService()
    {
        // Arrange
        var dto = new RejectAccommodationDto
        {
            Reason = "Missing information"
        };

        // Act
        IActionResult result =
            await _controller.Reject(5, dto);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.RejectAsync(5, "Missing information"),
            Times.Once);
    }

    [Fact]
    public async Task Suspend_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Suspend(5);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.SuspendAsync(5),
            Times.Once);
    }

    [Fact]
    public async Task Unsuspend_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Unsuspend(5);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.UnsuspendAsync(5),
            Times.Once);
    }
}
