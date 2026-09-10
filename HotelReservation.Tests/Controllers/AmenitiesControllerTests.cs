using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Amenity;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Enums;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AmenitiesControllerTests
{
    private const int OwnerId = 7;

    private readonly Mock<IAmenityService> _serviceMock;
    private readonly AmenitiesController _controller;

    public AmenitiesControllerTests()
    {
        _serviceMock = new Mock<IAmenityService>();
        _controller = new AmenitiesController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            OwnerId,
            "Owner");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithAmenities()
    {
        // Arrange
        var expected = new List<AmenityDto>
        {
            new()
            {
                Id = 1,
                Type = AmenityType.Wifi
            }
        };

        _serviceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AmenityDto>> result =
            await _controller.GetAll();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AmenityDto> response =
            Assert.IsType<List<AmenityDto>>(okResult.Value);

        Assert.Single(response);
        Assert.Equal(AmenityType.Wifi, response[0].Type);
    }

    [Fact]
    public async Task SetAccommodationAmenities_ShouldReturnOk_WithUpdatedAmenities()
    {
        // Arrange
        var dto = new UpdateAmenitiesDto
        {
            AmenityIds = new List<int> { 1, 2 }
        };

        var expected = new List<AmenityDto>
        {
            new() { Id = 1, Type = AmenityType.Wifi },
            new() { Id = 2, Type = AmenityType.Parking }
        };

        _serviceMock
            .Setup(s => s.SetAccommodationAmenitiesAsync(
                OwnerId,
                10,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AmenityDto>> result =
            await _controller.SetAccommodationAmenities(
                10,
                dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AmenityDto> response =
            Assert.IsType<List<AmenityDto>>(okResult.Value);

        Assert.Equal(2, response.Count);

        _serviceMock.Verify(
            s => s.SetAccommodationAmenitiesAsync(
                OwnerId,
                10,
                dto),
            Times.Once);
    }

    [Fact]
    public async Task SetUnitAmenities_ShouldReturnOk_WithUpdatedAmenities()
    {
        // Arrange
        var dto = new UpdateAmenitiesDto
        {
            AmenityIds = new List<int> { 1 }
        };

        var expected = new List<AmenityDto>
        {
            new() { Id = 1, Type = AmenityType.Wifi }
        };

        _serviceMock
            .Setup(s => s.SetUnitAmenitiesAsync(
                OwnerId,
                10,
                4,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AmenityDto>> result =
            await _controller.SetUnitAmenities(
                10,
                4,
                dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AmenityDto> response =
            Assert.IsType<List<AmenityDto>>(okResult.Value);

        Assert.Single(response);

        _serviceMock.Verify(
            s => s.SetUnitAmenitiesAsync(
                OwnerId,
                10,
                4,
                dto),
            Times.Once);
    }
}
