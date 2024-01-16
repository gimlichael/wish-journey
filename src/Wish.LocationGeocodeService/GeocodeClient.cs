using System;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cuemon.Extensions;
using Cuemon.Extensions.Net.Http;
using Microsoft.Extensions.Configuration;
using Wish.Services;

namespace Wish.LocationGeocodeService
{
    public class GeocodeClient : ILocationService
    {
        private readonly IConfiguration _configuration;

        public GeocodeClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Location> GetLocationAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
        {
            var locationApi = $"https://geocode.maps.co/reverse?lat={coordinates.Latitude.ToString("N", CultureInfo.InvariantCulture)}&lon={coordinates.Longitude.ToString("N", CultureInfo.InvariantCulture)}&api_key={_configuration["GeocodeApi"]}".ToUri();
            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            var response = await locationApi.HttpGetAsync(cancellationToken).ConfigureAwait(false);
            using (var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false))
            {

                return new Location
                {
                    City = GetCity(document.RootElement.GetProperty("address")),
                    Country = document.RootElement.GetProperty("address").GetProperty("country").GetString(),
                    Query = document.RootElement.GetProperty("display_name").GetString()
                };
            }
        }

        private string GetCity(JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty("city", out JsonElement city))
            {
                return city.GetString();
            }
            else if (jsonElement.TryGetProperty("town", out JsonElement town))
            {
                return town.GetString();
            }
            else if (jsonElement.TryGetProperty("village", out JsonElement village))
            {
                return village.GetString();
            }
            else if (jsonElement.TryGetProperty("hamlet", out JsonElement hamlet))
            {
                return hamlet.GetString();
            }
            else if (jsonElement.TryGetProperty("borough", out JsonElement borough))
            {
                return borough.GetString();
            }
            else if (jsonElement.TryGetProperty("suburb", out JsonElement suburb))
            {
                return suburb.GetString();
            }
            else if (jsonElement.TryGetProperty("district", out JsonElement district))
            {
                return district.GetString();
            }
            else if (jsonElement.TryGetProperty("municipality", out JsonElement municipality))
            {
	            return municipality.GetString();
            }
            else if (jsonElement.TryGetProperty("state", out JsonElement state))
            {
	            return state.GetString();
            }
            throw new JsonException($"Unable to locate city information from these 8 properties; city, town, suburb, hamlet, village, district, municipality and state. Here is the raw address: {jsonElement.GetRawText()}");
        }
    }
}
