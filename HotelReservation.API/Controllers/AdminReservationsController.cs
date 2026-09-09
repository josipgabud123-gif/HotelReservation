using System.Security.Claims;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/admin/reservations")]
[Authorize(Roles = "Admin")]
public class AdminReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public AdminReservationsController(
        IReservationService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ReservationDto>>>
        GetAll()
    {
        return Ok(
            await _service.GetAllAsync());
    }

    [HttpPost("{reservationId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int reservationId)
    {
        await _service.CancelByAdminAsync(
            AccountId,
            reservationId);

        return NoContent();
    }
}