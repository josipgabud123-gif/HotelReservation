using HotelReservation.API.Models.Requests;
using HotelReservation.Application.DTOs.Accommodation;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccommodationsController : ControllerBase
{
    private readonly IAccommodationService _service;

    public AccommodationsController(
        IAccommodationService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);
    [Authorize(Roles = "Owner")]
    [HttpPost]
    public async Task<ActionResult<AccommodationDto>> Create(
    CreateAccommodationDto dto)
    {
        AccommodationDto result =
            await _service.CreateAsync(
                AccountId,
                dto);

        return Ok(result);
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("my")]
    public async Task<ActionResult<List<AccommodationDto>>> My()
    {
        return Ok(
            await _service
                .GetOwnerAccommodationsAsync(
                    AccountId));
    }

    [Authorize(Roles = "Owner")]
    [HttpGet("{id:int}/manage")]
    public async Task<ActionResult<AccommodationManagementDto>> Manage(
        int id)
    {
        return Ok(
            await _service.GetManagementAsync(
                AccountId,
                id));
    }

    [Authorize(Roles = "Owner")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccommodationDto>> Update(
    int id,
    UpdateAccommodationDto dto)
    {
        return Ok(
            await _service.UpdateAsync(
                AccountId,
                id,
                dto));
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        await _service.SubmitForApprovalAsync(
            AccountId,
            id);

        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        await _service.ActivateAsync(
            AccountId,
            id);

        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _service.DeactivateAsync(
            AccountId,
            id);

        return NoContent();
    }

    [Authorize(Roles = "Owner")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(
            AccountId,
            id);

        return NoContent();
    }


    [Authorize(Roles = "Owner")]
    [HttpPost("{id:int}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AccommodationPhotoDto>> AddPhoto(
     int id,
     [FromForm] UploadPhotoRequest request)
    {
        if (request.File is null ||
            request.File.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        const long maxFileSize =
            5 * 1024 * 1024;

        if (request.File.Length > maxFileSize)
        {
            return BadRequest(
                "Maximum image size is 5 MB.");
        }

        string[] allowedContentTypes =
        {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

        if (!allowedContentTypes.Contains(
                request.File.ContentType.ToLowerInvariant()))
        {
            return BadRequest(
                "Unsupported image format.");
        }

        await using Stream stream =
            request.File.OpenReadStream();

        AccommodationPhotoDto result =
            await _service.AddPhotoAsync(
                AccountId,
                id,
                stream,
                request.File.FileName,
                request.IsPrimary);

        return Ok(result);
    }
    [Authorize(Roles = "Owner")]
    [HttpDelete("{id:int}/photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(
    int id,
    int photoId)
    {
        await _service.DeletePhotoAsync(
            AccountId,
            id,
            photoId);

        return NoContent();
    }
    [Authorize(Roles = "Owner")]
    [HttpPost("{id:int}/photos/{photoId:int}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(
    int id,
    int photoId)
    {
        await _service.SetPrimaryPhotoAsync(
            AccountId,
            id,
            photoId);

        return NoContent();
    }
}