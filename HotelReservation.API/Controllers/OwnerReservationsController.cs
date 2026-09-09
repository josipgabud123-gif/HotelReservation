using System.Security.Claims;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/owner/reservations")]
[Authorize(Roles = "Owner")]
public class OwnerReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public OwnerReservationsController(
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
        GetReservations()
    {
        return Ok(
            await _service
                .GetOwnerReservationsAsync(
                    AccountId));
    }

    [HttpPost("{reservationId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int reservationId)
    {
        await _service.CancelByOwnerAsync(
            AccountId,
            reservationId);

        return NoContent();
    }
}