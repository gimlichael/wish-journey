namespace Wish.Services
{
    public interface IWeatherService
    {
        Task<Weather> GetWeatherAsync(Coordinates coordinates, CancellationToken cancellationToken = default);
    }
}
