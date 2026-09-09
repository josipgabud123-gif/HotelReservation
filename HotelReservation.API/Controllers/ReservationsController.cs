using System.Security.Claims;
using HotelReservation.Application.DTOs.Reservation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize(Roles = "Customer,Owner")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(
        IReservationService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ReservationDto>>
        Create(
            CreateReservationDto dto)
    {
        ReservationDto result =
            await _service.CreateAsync(
                AccountId,
                dto);

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ReservationDto>>>
        GetMyReservations()
    {
        return Ok(
            await _service
                .GetMyReservationsAsync(
                    AccountId));
    }

    [HttpPost("{reservationId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int reservationId)
    {
        await _service.CancelByGuestAsync(
            AccountId,
            reservationId);

        return NoContent();
    }
}