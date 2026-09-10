using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.AvailabilityBlock;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AvailabilityBlocksControllerTests
{
    private const int OwnerId = 7;

    private readonly Mock<IAvailabilityBlockService> _serviceMock;
    private readonly AvailabilityBlocksController _controller;

    public AvailabilityBlocksControllerTests()
    {
        _serviceMock = new Mock<IAvailabilityBlockService>();
        _controller = new AvailabilityBlocksController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            OwnerId,
            "Owner");
    }

    [Fact]
    public async Task CreateForAccommodation_ShouldReturnOk_WithCreatedBlock()
    {
        // Arrange
        var dto = new CreateAvailabilityBlockDto
        {
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 5)
        };

        var expected = new AvailabilityBlockDto
        {
            Id = 1,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AccommodationId = 10,
            CreatedByOwnerId = OwnerId
        };

        _serviceMock
            .Setup(s => s.CreateForAccommodationAsync(
                OwnerId,
                10,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AvailabilityBlockDto> result =
            await _controller.CreateForAccommodation(
                10,
                dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AvailabilityBlockDto response =
            Assert.IsType<AvailabilityBlockDto>(okResult.Value);

        Assert.Equal(1, response.Id);
        Assert.Equal(10, response.AccommodationId);
    }

    [Fact]
    public async Task CreateForUnit_ShouldReturnOk_WithCreatedBlock()
    {
        // Arrange
        var dto = new CreateAvailabilityBlockDto
        {
            StartDate = new DateOnly(2026, 12, 1),
            EndDate = new DateOnly(2026, 12, 3)
        };

        var expected = new AvailabilityBlockDto
        {
            Id = 2,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AccommodationUnitId = 4,
            CreatedByOwnerId = OwnerId
        };

        _serviceMock
            .Setup(s => s.CreateForUnitAsync(
                OwnerId,
                10,
                4,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AvailabilityBlockDto> result =
            await _controller.CreateForUnit(
                10,
                4,
                dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AvailabilityBlockDto response =
            Assert.IsType<AvailabilityBlockDto>(okResult.Value);

        Assert.Equal(4, response.AccommodationUnitId);
    }

    [Fact]
    public async Task GetAccommodationBlocks_ShouldReturnOk_WithBlocks()
    {
        // Arrange
        var expected = new List<AvailabilityBlockDto>
        {
            new() { Id = 1, AccommodationId = 10 }
        };

        _serviceMock
            .Setup(s => s.GetAccommodationBlocksAsync(
                OwnerId,
                10,
                false))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AvailabilityBlockDto>> result =
            await _controller.GetAccommodationBlocks(
                10,
                false);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AvailabilityBlockDto> response =
            Assert.IsType<List<AvailabilityBlockDto>>(okResult.Value);

        Assert.Single(response);
    }

    [Fact]
    public async Task GetUnitBlocks_ShouldReturnOk_WithBlocks()
    {
        // Arrange
        var expected = new List<AvailabilityBlockDto>
        {
            new() { Id = 2, AccommodationUnitId = 4 }
        };

        _serviceMock
            .Setup(s => s.GetUnitBlocksAsync(
                OwnerId,
                10,
                4,
                true))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AvailabilityBlockDto>> result =
            await _controller.GetUnitBlocks(
                10,
                4,
                true);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AvailabilityBlockDto> response =
            Assert.IsType<List<AvailabilityBlockDto>>(okResult.Value);

        Assert.Single(response);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WithUpdatedBlock()
    {
        // Arrange
        var dto = new UpdateAvailabilityBlockDto
        {
            StartDate = new DateOnly(2026, 12, 5),
            EndDate = new DateOnly(2026, 12, 10)
        };

        var expected = new AvailabilityBlockDto
        {
            Id = 3,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            AccommodationId = 10
        };

        _serviceMock
            .Setup(s => s.UpdateAsync(
                OwnerId,
                3,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AvailabilityBlockDto> result =
            await _controller.Update(3, dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AvailabilityBlockDto response =
            Assert.IsType<AvailabilityBlockDto>(okResult.Value);

        Assert.Equal(dto.EndDate, response.EndDate);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Delete(3);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeleteAsync(OwnerId, 3),
            Times.Once);
    }
}
