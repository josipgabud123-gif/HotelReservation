using HotelReservation.Application.DTOs.Accommodation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/admin/accommodations")]
[Authorize(Roles = "Admin")]
public class AdminAccommodationsController : ControllerBase
{
    private readonly IAccommodationService
        _accommodationService;

    public AdminAccommodationsController(
        IAccommodationService accommodationService)
    {
        _accommodationService = accommodationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccommodationDto>>>
        GetAll()
    {
        return Ok(
            await _accommodationService.GetAllAsync());
    }

    [HttpGet("{id:int}/details")]
    public async Task<ActionResult<AccommodationManagementDto>> GetDetails(
        int id)
    {
        return Ok(
            await _accommodationService.GetAdminDetailsAsync(id));
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<AccommodationDto>>>
        GetPending()
    {
        return Ok(
            await _accommodationService.GetPendingAsync());
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        await _accommodationService.ApproveAsync(id);

        return NoContent();
    }

   
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(
     int id,
     RejectAccommodationDto dto)
    {
        await _accommodationService.RejectAsync(
            id,
            dto.Reason);

        return NoContent();
    }

    [HttpPost("{id:int}/suspend")]
    public async Task<IActionResult> Suspend(int id)
    {
        await _accommodationService.SuspendAsync(id);

        return NoContent();
    }

    [HttpPost("{id:int}/unsuspend")]
    public async Task<IActionResult> Unsuspend(int id)
    {
        await _accommodationService.UnsuspendAsync(id);

        return NoContent();
    }
}