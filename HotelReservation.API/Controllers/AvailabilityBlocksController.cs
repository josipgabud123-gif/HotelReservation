using System.Security.Claims;
using HotelReservation.Application.DTOs.AvailabilityBlock;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api")]
[Authorize(Roles = "Owner")]
public class AvailabilityBlocksController : ControllerBase
{
    private readonly IAvailabilityBlockService _service;

    public AvailabilityBlocksController(
        IAvailabilityBlockService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [HttpPost(
        "accommodations/{accommodationId:int}/availability-blocks")]
    public async Task<ActionResult<AvailabilityBlockDto>>
        CreateForAccommodation(
            int accommodationId,
            CreateAvailabilityBlockDto dto)
    {
        AvailabilityBlockDto result =
            await _service
                .CreateForAccommodationAsync(
                    AccountId,
                    accommodationId,
                    dto);

        return Ok(result);
    }

    [HttpPost(
        "accommodations/{accommodationId:int}/units/{unitId:int}/availability-blocks")]
    public async Task<ActionResult<AvailabilityBlockDto>>
        CreateForUnit(
            int accommodationId,
            int unitId,
            CreateAvailabilityBlockDto dto)
    {
        AvailabilityBlockDto result =
            await _service
                .CreateForUnitAsync(
                    AccountId,
                    accommodationId,
                    unitId,
                    dto);

        return Ok(result);
    }

    [HttpGet(
        "accommodations/{accommodationId:int}/availability-blocks")]
    public async Task<ActionResult<List<AvailabilityBlockDto>>>
        GetAccommodationBlocks(
            int accommodationId,
            [FromQuery] bool includePast = false)
    {
        return Ok(
            await _service
                .GetAccommodationBlocksAsync(
                    AccountId,
                    accommodationId,
                    includePast));
    }

    [HttpGet(
        "accommodations/{accommodationId:int}/units/{unitId:int}/availability-blocks")]
    public async Task<ActionResult<List<AvailabilityBlockDto>>>
        GetUnitBlocks(
            int accommodationId,
            int unitId,
            [FromQuery] bool includePast = false)
    {
        return Ok(
            await _service
                .GetUnitBlocksAsync(
                    AccountId,
                    accommodationId,
                    unitId,
                    includePast));
    }

    [HttpPut("availability-blocks/{blockId:int}")]
    public async Task<ActionResult<AvailabilityBlockDto>>
        Update(
            int blockId,
            UpdateAvailabilityBlockDto dto)
    {
        return Ok(
            await _service.UpdateAsync(
                AccountId,
                blockId,
                dto));
    }

    [HttpDelete("availability-blocks/{blockId:int}")]
    public async Task<IActionResult> Delete(
        int blockId)
    {
        await _service.DeleteAsync(
            AccountId,
            blockId);

        return NoContent();
    }
}