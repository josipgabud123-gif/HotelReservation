using HotelReservation.Application.DTOs.Search;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/accommodations")]
public class AccommodationSearchController
    : ControllerBase
{
    private readonly IAccommodationSearchService
        _searchService;

    public AccommodationSearchController(
        IAccommodationSearchService searchService)
    {
        _searchService = searchService;
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<
        ActionResult<List<AccommodationSearchResultDto>>>
        Search(
            [FromQuery] AccommodationSearchDto dto)
    {
        List<AccommodationSearchResultDto> results =
            await _searchService.SearchAsync(dto);

        return Ok(results);
    }
}