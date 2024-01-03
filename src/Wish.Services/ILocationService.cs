namespace Wish.Services
{
    public interface ILocationService
    {
        Task<Location> GetLocationAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
    }
}
