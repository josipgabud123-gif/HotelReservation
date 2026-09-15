using HotelReservation.API.Controllers;
using HotelReservation.API.Models.Requests;
using HotelReservation.Application.DTOs.AccommodationUnit;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Enums;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AccommodationUnitsControllerTests
{
    private const int OwnerId = 7;

    private readonly Mock<IAccommodationUnitService> _serviceMock;
    private readonly AccommodationUnitsController _controller;

    public AccommodationUnitsControllerTests()
    {
        _serviceMock = new Mock<IAccommodationUnitService>();
        _controller = new AccommodationUnitsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            OwnerId,
            "Owner");
    }

    [Fact]
    public async Task Create_WhenNoPhotos_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateAccommodationUnitRequest
        {
            Name = "Soba 101",
            Capacity = 2,
            PricePerNight = 150,
            CancellationPolicy = CancellationPolicy.Flexible,
            Photos = new List<Microsoft.AspNetCore.Http.IFormFile>()
        };

        // Act
        ActionResult<AccommodationUnitDto> result =
            await _controller.Create(10, request);

        // Assert
        BadRequestObjectResult badRequest =
            Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal(
            "At least one photo is required.",
            badRequest.Value);

        _serviceMock.Verify(
            s => s.CreateAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CreateAccommodationUnitDto>(),
                It.IsAny<IReadOnlyList<ImageUploadData>>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_WithValidPhoto_ShouldReturnOk()
    {
        // Arrange
        var request = new CreateAccommodationUnitRequest
        {
            Name = "Soba 101",
            Capacity = 2,
            PricePerNight = 150,
            CancellationPolicy = CancellationPolicy.Flexible,
            PrimaryPhotoIndex = 0,
            Photos = new List<Microsoft.AspNetCore.Http.IFormFile>
            {
                ControllerTestHelper.CreateFormFile()
            }
        };

        var expected = new AccommodationUnitDto
        {
            Id = 4,
            Name = "Soba 101",
            Capacity = 2,
            PricePerNight = 150,
            AccommodationId = 10
        };

        _serviceMock
            .Setup(s => s.CreateAsync(
                OwnerId,
                10,
                It.Is<CreateAccommodationUnitDto>(d =>
                    d.Name == "Soba 101" &&
                    d.Capacity == 2 &&
                    d.PricePerNight == 150),
                It.Is<IReadOnlyList<ImageUploadData>>(images =>
                    images.Count == 1 &&
                    images[0].FileName == "test.jpg"),
                0))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationUnitDto> result =
            await _controller.Create(10, request);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationUnitDto response =
            Assert.IsType<AccommodationUnitDto>(okResult.Value);

        Assert.Equal(4, response.Id);
        Assert.Equal("Soba 101", response.Name);

        _serviceMock.Verify(
            s => s.CreateAsync(
                OwnerId,
                10,
                It.IsAny<CreateAccommodationUnitDto>(),
                It.IsAny<IReadOnlyList<ImageUploadData>>(),
                0),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WithUpdatedUnit()
    {
        // Arrange
        var dto = new UpdateAccommodationUnitDto
        {
            Name = "Soba 102",
            Capacity = 3,
            PricePerNight = 180,
            CancellationPolicy = CancellationPolicy.Moderate
        };

        var expected = new AccommodationUnitDto
        {
            Id = 4,
            Name = dto.Name,
            Capacity = dto.Capacity,
            PricePerNight = dto.PricePerNight,
            AccommodationId = 10
        };

        _serviceMock
            .Setup(s => s.UpdateAsync(
                OwnerId,
                10,
                4,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationUnitDto> result =
            await _controller.Update(
                10,
                4,
                dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationUnitDto response =
            Assert.IsType<AccommodationUnitDto>(okResult.Value);

        Assert.Equal("Soba 102", response.Name);
        Assert.Equal(180, response.PricePerNight);
    }

    [Fact]
    public async Task Activate_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Activate(10, 4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.ActivateAsync(OwnerId, 10, 4),
            Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Deactivate(10, 4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeactivateAsync(OwnerId, 10, 4),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.Delete(10, 4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeleteAsync(OwnerId, 10, 4),
            Times.Once);
    }
}
