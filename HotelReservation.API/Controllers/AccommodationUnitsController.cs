using System.Security.Claims;
using HotelReservation.API.Models.Requests;
using HotelReservation.Application.DTOs.AccommodationUnit;
using HotelReservation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.API.Controllers;

[ApiController]
[Route("api/accommodations/{accommodationId:int}/units")]
[Authorize(Roles = "Owner")]
public class AccommodationUnitsController : ControllerBase
{
    private readonly IAccommodationUnitService _service;

    public AccommodationUnitsController(
        IAccommodationUnitService service)
    {
        _service = service;
    }

    private int AccountId =>
        int.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AccommodationUnitDto>> Create(
        int accommodationId,
        [FromForm] CreateAccommodationUnitRequest request)
    {
        if (request.Photos.Count == 0)
        {
            return BadRequest(
                "At least one photo is required.");
        }

        const long maxFileSize =
            5 * 1024 * 1024;

        string[] allowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        foreach (IFormFile file in request.Photos)
        {
            if (file.Length == 0)
            {
                return BadRequest(
                    "Photo cannot be empty.");
            }

            if (file.Length > maxFileSize)
            {
                return BadRequest(
                    "Maximum image size is 5 MB.");
            }

            if (!allowedContentTypes.Contains(
                    file.ContentType.ToLowerInvariant()))
            {
                return BadRequest(
                    "Unsupported image format.");
            }
        }

        var streams = new List<Stream>();

        try
        {
            var images =
                new List<ImageUploadData>();

            foreach (IFormFile file in request.Photos)
            {
                Stream stream =
                    file.OpenReadStream();

                streams.Add(stream);

                images.Add(
                    new ImageUploadData
                    {
                        Stream = stream,
                        FileName = file.FileName
                    });
            }

            var dto =
                new CreateAccommodationUnitDto
                {
                    Name = request.Name,
                    Capacity = request.Capacity,
                    PricePerNight =
                        request.PricePerNight,
                    CancellationPolicy =
                        request.CancellationPolicy
                };

            AccommodationUnitDto result =
                await _service.CreateAsync(
                    AccountId,
                    accommodationId,
                    dto,
                    images,
                    request.PrimaryPhotoIndex);

            return Ok(result);
        }
        finally
        {
            foreach (Stream stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    [HttpPost("{unitId:int}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<AccommodationUnitPhotoDto>> AddPhoto(
        int accommodationId,
        int unitId,
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

        AccommodationUnitPhotoDto result =
            await _service.AddPhotoAsync(
                AccountId,
                accommodationId,
                unitId,
                stream,
                request.File.FileName,
                request.IsPrimary);

        return Ok(result);
    }

    [HttpDelete("{unitId:int}/photos/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(
        int accommodationId,
        int unitId,
        int photoId)
    {
        await _service.DeletePhotoAsync(
            AccountId,
            accommodationId,
            unitId,
            photoId);

        return NoContent();
    }

    [HttpPost("{unitId:int}/photos/{photoId:int}/primary")]
    public async Task<IActionResult> SetPrimaryPhoto(
        int accommodationId,
        int unitId,
        int photoId)
    {
        await _service.SetPrimaryPhotoAsync(
            AccountId,
            accommodationId,
            unitId,
            photoId);

        return NoContent();
    }

    [HttpPut("{unitId:int}")]
    public async Task<ActionResult<AccommodationUnitDto>> Update(
        int accommodationId,
        int unitId,
        UpdateAccommodationUnitDto dto)
    {
        AccommodationUnitDto result =
            await _service.UpdateAsync(
                AccountId,
                accommodationId,
                unitId,
                dto);

        return Ok(result);
    }

    [HttpPost("{unitId:int}/activate")]
    public async Task<IActionResult> Activate(
        int accommodationId,
        int unitId)
    {
        await _service.ActivateAsync(
            AccountId,
            accommodationId,
            unitId);

        return NoContent();
    }

    [HttpPost("{unitId:int}/deactivate")]
    public async Task<IActionResult> Deactivate(
        int accommodationId,
        int unitId)
    {
        await _service.DeactivateAsync(
            AccountId,
            accommodationId,
            unitId);

        return NoContent();
    }

    [HttpDelete("{unitId:int}")]
    public async Task<IActionResult> Delete(
        int accommodationId,
        int unitId)
    {
        await _service.DeleteAsync(
            AccountId,
            accommodationId,
            unitId);

        return NoContent();
    }
}