using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelReservation.Tests.Helpers;

public static class ControllerTestHelper
{
    public static void SetUser(
        ControllerBase controller,
        int accountId,
        string role = "Owner")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    public static IFormFile CreateFormFile(
        string fileName = "test.jpg",
        string contentType = "image/jpeg",
        int size = 10)
    {
        byte[] content = new byte[size];
        var stream = new MemoryStream(content);

        return new FormFile(
            stream,
            0,
            stream.Length,
            "file",
            fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
