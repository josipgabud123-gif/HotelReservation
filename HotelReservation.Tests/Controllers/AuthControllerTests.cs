using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Auth;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock =
            new Mock<IAuthService>();

        _controller =
            new AuthController(
                _authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WithAuthResponse()
    {
        // Arrange
        var dto = new RegisterDto
        {
            FirstName = "Ivan",
            LastName = "Ivic",
            Email = "ivan@test.com",
            Password = "Test1234"
        };

        var expected = new AuthResponseDto
        {
            Token = "test-token"
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AuthResponseDto> result =
            await _controller.Register(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        AuthResponseDto response =
            Assert.IsType<AuthResponseDto>(
                okResult.Value);

        Assert.Equal(
            expected.Token,
            response.Token);

        _authServiceMock.Verify(
            s => s.RegisterAsync(dto),
            Times.Once);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WithAuthResponse()
    {
        // Arrange
        var dto = new LoginDto
        {
            Email = "ivan@test.com",
            Password = "Test1234"
        };

        var expected = new AuthResponseDto
        {
            Token = "test-token"
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AuthResponseDto> result =
            await _controller.Login(dto);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(
                result.Result);

        AuthResponseDto response =
            Assert.IsType<AuthResponseDto>(
                okResult.Value);

        Assert.Equal(
            expected.Token,
            response.Token);

        _authServiceMock.Verify(
            s => s.LoginAsync(dto),
            Times.Once);
    }
}