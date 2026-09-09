using System.Security.Claims;
using HotelReservation.Application.DTOs.Amenity;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(
        IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpGet("amenities")]
    public async Task<ActionResult<List<AmenityDto>>>
        GetAll()
    {
        return Ok(
            await _amenityService
                .GetAllAsync());
    }

    [Authorize(Roles = "Owner")]
    [HttpPut(
        "accommodations/{accommodationId:int}/amenities")]
    public async Task<ActionResult<List<AmenityDto>>>
        SetAccommodationAmenities(
            int accommodationId,
            UpdateAmenitiesDto dto)
    {
        List<AmenityDto> result =
            await _amenityService
                .SetAccommodationAmenitiesAsync(
                    AccountId,
                    accommodationId,
                    dto);

        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpPut(
        "accommodations/{accommodationId:int}/units/{unitId:int}/amenities")]
    public async Task<ActionResult<List<AmenityDto>>>
        SetUnitAmenities(
            int accommodationId,
            int unitId,
            UpdateAmenitiesDto dto)
    {
        List<AmenityDto> result =
            await _amenityService
                .SetUnitAmenitiesAsync(
                    AccountId,
                    accommodationId,
                    unitId,
                    dto);

        return Ok(result);
    }
}