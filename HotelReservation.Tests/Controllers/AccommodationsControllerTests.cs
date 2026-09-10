using HotelReservation.API.Controllers;
using HotelReservation.API.Models.Requests;
using HotelReservation.Application.DTOs.Accommodation;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Enums;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AccommodationsControllerTests
{
    private const int OwnerId = 7;

    private readonly Mock<IAccommodationService> _serviceMock;
    private readonly AccommodationsController _controller;

    public AccommodationsControllerTests()
    {
        _serviceMock = new Mock<IAccommodationService>();
        _controller = new AccommodationsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            OwnerId,
            "Owner");
    }

    [Fact]
    public async Task Create_ShouldReturnOk_WithCreatedAccommodation()
    {
        // Arrange
        var dto = new CreateAccommodationDto
        {
            Name = "Hotel Test",
            Description = "Opis",
            Address = "Test 1",
            City = "Split",
            Country = "Croatia",
            Type = AccommodationType.Hotel
        };

        var expected = new AccommodationDto
        {
            Id = 10,
            Name = "Hotel Test",
            OwnerId = OwnerId
        };

        _serviceMock
            .Setup(s => s.CreateAsync(
                OwnerId,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationDto> result =
            await _controller.Create(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationDto response =
            Assert.IsType<AccommodationDto>(okResult.Value);

        Assert.Equal(10, response.Id);
        Assert.Equal("Hotel Test", response.Name);
    }

    [Fact]
    public async Task My_ShouldReturnOk_WithOwnerAccommodations()
    {
        // Arrange
        var expected = new List<AccommodationDto>
        {
            new() { Id = 10, OwnerId = OwnerId }
        };

        _serviceMock
            .Setup(s => s.GetOwnerAccommodationsAsync(OwnerId))
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AccommodationDto>> result =
            await _controller.My();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AccommodationDto> response =
            Assert.IsType<List<AccommodationDto>>(okResult.Value);

        Assert.Single(response);
    }

    [Fact]
    public async Task Manage_ShouldReturnOk_WithManagementData()
    {
        // Arrange
        var expected = new AccommodationManagementDto
        {
            Id = 10,
            Name = "Hotel Test"
        };

        _serviceMock
            .Setup(s => s.GetManagementAsync(
                OwnerId,
                10))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationManagementDto> result =
            await _controller.Manage(10);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationManagementDto response =
            Assert.IsType<AccommodationManagementDto>(
                okResult.Value);

        Assert.Equal(10, response.Id);
        Assert.Equal("Hotel Test", response.Name);

        _serviceMock.Verify(
            s => s.GetManagementAsync(OwnerId, 10),
            Times.Once);
    }

    [Fact]
    public async Task Update_ShouldReturnOk_WithUpdatedAccommodation()
    {
        // Arrange
        var dto = new UpdateAccommodationDto
        {
            Name = "Novi naziv",
            Description = "Novi opis"
        };

        var expected = new AccommodationDto
        {
            Id = 10,
            Name = "Novi naziv",
            Description = "Novi opis"
        };

        _serviceMock
            .Setup(s => s.UpdateAsync(
                OwnerId,
                10,
                dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationDto> result =
            await _controller.Update(10, dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationDto response =
            Assert.IsType<AccommodationDto>(okResult.Value);

        Assert.Equal("Novi naziv", response.Name);
    }

    [Fact]
    public async Task Submit_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Submit(10);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.SubmitForApprovalAsync(OwnerId, 10),
            Times.Once);
    }

    [Fact]
    public async Task Activate_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Activate(10);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.ActivateAsync(OwnerId, 10),
            Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Deactivate(10);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeactivateAsync(OwnerId, 10),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result = await _controller.Delete(10);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeleteAsync(OwnerId, 10),
            Times.Once);
    }

    [Fact]
    public async Task AddPhoto_WhenFileIsEmpty_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UploadPhotoRequest
        {
            File = ControllerTestHelper.CreateFormFile(size: 0),
            IsPrimary = true
        };

        // Act
        ActionResult<AccommodationPhotoDto> result =
            await _controller.AddPhoto(10, request);

        // Assert
        BadRequestObjectResult badRequest =
            Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal("File is empty.", badRequest.Value);

        _serviceMock.Verify(
            s => s.AddPhotoAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task AddPhoto_WithValidFile_ShouldReturnOk()
    {
        // Arrange
        var request = new UploadPhotoRequest
        {
            File = ControllerTestHelper.CreateFormFile(),
            IsPrimary = true
        };

        var expected = new AccommodationPhotoDto
        {
            Id = 4,
            Path = "/uploads/test.jpg",
            IsPrimary = true
        };

        _serviceMock
            .Setup(s => s.AddPhotoAsync(
                OwnerId,
                10,
                It.IsAny<Stream>(),
                "test.jpg",
                true))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccommodationPhotoDto> result =
            await _controller.AddPhoto(10, request);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccommodationPhotoDto response =
            Assert.IsType<AccommodationPhotoDto>(okResult.Value);

        Assert.Equal(4, response.Id);
        Assert.True(response.IsPrimary);

        _serviceMock.Verify(
            s => s.AddPhotoAsync(
                OwnerId,
                10,
                It.IsAny<Stream>(),
                "test.jpg",
                true),
            Times.Once);
    }

    [Fact]
    public async Task DeletePhoto_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.DeletePhoto(10, 4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.DeletePhotoAsync(OwnerId, 10, 4),
            Times.Once);
    }

    [Fact]
    public async Task SetPrimaryPhoto_ShouldReturnNoContent_AndCallService()
    {
        // Act
        IActionResult result =
            await _controller.SetPrimaryPhoto(10, 4);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _serviceMock.Verify(
            s => s.SetPrimaryPhotoAsync(OwnerId, 10, 4),
            Times.Once);
    }
}
