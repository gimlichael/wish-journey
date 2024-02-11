using System;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cuemon.Extensions;
using Cuemon.Extensions.Net.Http;
using Wish.Services;

namespace Wish.IpGeolocationService
{
    public class TimeZoneClient : ITimestampService
    {
        private readonly TimeZoneClientOptions _options;

        public TimeZoneClient(TimeZoneClientOptions options)
        {
            _options = options;
        }

        public async Task<Timestamp> GetTimestampAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
        {
            var timezoneApi = $"https://api.ipgeolocation.io/timezone?apiKey={_options.ApiKey}&lat={coordinates.Latitude.ToString("N", CultureInfo.InvariantCulture)}&long={coordinates.Longitude.ToString("N", CultureInfo.InvariantCulture)}".ToUri();
            var response = await timezoneApi.HttpGetAsync(cancellationToken).ConfigureAwait(false);
            using (var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return new Timestamp(DateTimeOffset.Parse(document.RootElement.GetProperty("date_time_ymd").GetString()))
                {
                    IsDaylightSavingTime = document.RootElement.GetProperty("is_dst").GetBoolean(),
                    TimeZone = document.RootElement.GetProperty("timezone").GetString()
                };
            }
        }

        public async Task<Timestamp> GetTimestampAsync(string ianaTimeZoneName, CancellationToken cancellationToken = default)
        {
            var timezoneApi = $"https://api.ipgeolocation.io/timezone?apiKey={_options.ApiKey}&tz={ianaTimeZoneName}".ToUri();
            var response = await timezoneApi.HttpGetAsync(cancellationToken).ConfigureAwait(false);
            using (var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                return new Timestamp(DateTimeOffset.Parse(document.RootElement.GetProperty("date_time_ymd").GetString()))
                {
                    IsDaylightSavingTime = document.RootElement.GetProperty("is_dst").GetBoolean(),
                    TimeZone = document.RootElement.GetProperty("timezone").GetString()
                };
            }
        }
    }
}
