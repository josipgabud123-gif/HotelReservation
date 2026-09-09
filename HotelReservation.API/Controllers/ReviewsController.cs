using System.Security.Claims;
using HotelReservation.Application.DTOs.Review;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewsController(
        IReviewService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpGet(
        "accommodations/{accommodationId:int}/reviews")]
    public async Task<
        ActionResult<AccommodationReviewsDto>>
        GetByAccommodation(
            int accommodationId)
    {
        return Ok(
            await _service
                .GetByAccommodationAsync(
                    accommodationId));
    }

    [Authorize(Roles = "Customer,Owner")]
    [HttpPost("reviews")]
    public async Task<ActionResult<ReviewDto>>
        Create(
            CreateReviewDto dto)
    {
        return Ok(
            await _service.CreateAsync(
                AccountId,
                dto));
    }

    [Authorize(Roles = "Customer,Owner")]
    [HttpPut("reviews/{reviewId:int}")]
    public async Task<ActionResult<ReviewDto>>
        Update(
            int reviewId,
            UpdateReviewDto dto)
    {
        return Ok(
            await _service.UpdateAsync(
                AccountId,
                reviewId,
                dto));
    }

    [Authorize(Roles = "Customer,Owner")]
    [HttpDelete("reviews/{reviewId:int}")]
    public async Task<IActionResult> DeleteOwn(
        int reviewId)
    {
        await _service.DeleteOwnAsync(
            AccountId,
            reviewId);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete(
        "admin/reviews/{reviewId:int}")]
    public async Task<IActionResult> DeleteByAdmin(
        int reviewId)
    {
        await _service.DeleteByAdminAsync(
            AccountId,
            reviewId);

        return NoContent();
    }
}