namespace HotelReservation.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream fileStream,
        string fileName,
        string folder);

    void Delete(string path);
}