using HotelReservation.API.Controllers;
using HotelReservation.Application.DTOs.Account;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Domain.Enums;
using HotelReservation.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelReservation.Tests.Controllers;

public class AdminAccountsControllerTests
{
    private const int AdminId = 1;

    private readonly Mock<IAccountManagementService> _serviceMock;
    private readonly AdminAccountsController _controller;

    public AdminAccountsControllerTests()
    {
        _serviceMock = new Mock<IAccountManagementService>();
        _controller = new AdminAccountsController(
            _serviceMock.Object);

        ControllerTestHelper.SetUser(
            _controller,
            AdminId,
            "Admin");
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithAccounts()
    {
        // Arrange
        var expected = new List<AccountDto>
        {
            new()
            {
                Id = 2,
                FirstName = "Ivan",
                LastName = "Ivic",
                Email = "ivan@test.com",
                Role = UserRole.Customer,
                IsActive = true
            }
        };

        _serviceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(expected);

        // Act
        ActionResult<List<AccountDto>> result =
            await _controller.GetAll();

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        List<AccountDto> response =
            Assert.IsType<List<AccountDto>>(okResult.Value);

        Assert.Single(response);
        Assert.Equal(2, response[0].Id);

        _serviceMock.Verify(
            s => s.GetAllAsync(),
            Times.Once);
    }

    [Fact]
    public async Task Activate_ShouldReturnOk_AndSetStatusToTrue()
    {
        // Arrange
        var expected = new AccountDto
        {
            Id = 2,
            IsActive = true
        };

        _serviceMock
            .Setup(s => s.SetActiveStatusAsync(
                AdminId,
                2,
                true))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccountDto> result =
            await _controller.Activate(2);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccountDto response =
            Assert.IsType<AccountDto>(okResult.Value);

        Assert.True(response.IsActive);

        _serviceMock.Verify(
            s => s.SetActiveStatusAsync(
                AdminId,
                2,
                true),
            Times.Once);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnOk_AndSetStatusToFalse()
    {
        // Arrange
        var expected = new AccountDto
        {
            Id = 2,
            IsActive = false
        };

        _serviceMock
            .Setup(s => s.SetActiveStatusAsync(
                AdminId,
                2,
                false))
            .ReturnsAsync(expected);

        // Act
        ActionResult<AccountDto> result =
            await _controller.Deactivate(2);

        // Assert
        OkObjectResult okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        AccountDto response =
            Assert.IsType<AccountDto>(okResult.Value);

        Assert.False(response.IsActive);

        _serviceMock.Verify(
            s => s.SetActiveStatusAsync(
                AdminId,
                2,
                false),
            Times.Once);
    }
}
