using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Interfaces.Services;

namespace HotelReservation.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public LocalFileStorageService(string webRootPath)
    {
        _webRootPath = webRootPath;
    }

    public async Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string folder)
    {
        string extension =
            Path.GetExtension(fileName).ToLowerInvariant();

        string[] allowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedExtensions.Contains(extension))
        {
            throw new ValidationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        string folderPath = Path.Combine(
            _webRootPath,
            "uploads",
            folder);

        Directory.CreateDirectory(folderPath);

        string generatedFileName =
            $"{Guid.NewGuid()}{extension}";

        string fullPath =
            Path.Combine(folderPath, generatedFileName);

        await using var outputStream =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write);

        await fileStream.CopyToAsync(outputStream);

        return $"/uploads/{folder}/{generatedFileName}";
    }

    public void Delete(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        string relativePath = path
            .TrimStart('/')
            .Replace(
                '/',
                Path.DirectorySeparatorChar);

        string fullPath =
            Path.GetFullPath(
                Path.Combine(_webRootPath, relativePath));

        string webRootFullPath =
            Path.GetFullPath(_webRootPath);

        if (!fullPath.StartsWith(
                webRootFullPath,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}