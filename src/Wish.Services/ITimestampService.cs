namespace Wish.Services
{
    public interface ITimestampService
    {
        Task<Timestamp> GetTimestampAsync(Coordinates coordinates, CancellationToken cancellationToken = default);

        Task<Timestamp> GetTimestampAsync(string ianaTimeZoneName, CancellationToken cancellationToken = default);
    }
}
