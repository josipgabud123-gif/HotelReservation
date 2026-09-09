using System.Security.Claims;
using HotelReservation.Application.DTOs.Account;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = "Admin")]
public class AdminAccountsController
    : ControllerBase
{
    private readonly IAccountManagementService _service;

    public AdminAccountsController(
        IAccountManagementService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>>
        GetAll()
    {
        return Ok(
            await _service.GetAllAsync());
    }

    [HttpPost("{accountId:int}/activate")]
    public async Task<ActionResult<AccountDto>>
        Activate(
            int accountId)
    {
        return Ok(
            await _service.SetActiveStatusAsync(
                AccountId,
                accountId,
                true));
    }

    [HttpPost("{accountId:int}/deactivate")]
    public async Task<ActionResult<AccountDto>>
        Deactivate(
            int accountId)
    {
        return Ok(
            await _service.SetActiveStatusAsync(
                AccountId,
                accountId,
                false));
    }
}