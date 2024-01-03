using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cuemon.Extensions;
using Cuemon.Extensions.Net.Http;
using Microsoft.Extensions.Configuration;
using Wish.Services;

namespace Wish.WeatherTomorrowService
{
    public class WeatherClient : IWeatherService
    {
        private readonly IConfiguration _configuration;
        private static readonly Dictionary<int, string> WeatherCodes = new() // https://docs.tomorrow.io/reference/data-layers-weather-codes
        {
            { 0, "Unknown" },
            { 1000, "Clear, Sunny" },
            { 1100, "Mostly Clear" },
            { 1101, "Partly Cloudy" },
            { 1102, "Mostly Cloudy" },
            { 1001, "Cloudy" },
            { 2000, "Fog" },
            { 2100, "Light Fog" },
            { 4000, "Drizzle" },
            { 4001, "Rain" },
            { 4200, "Light Rain" },
            { 4201, "Heavy Rain" },
            { 5000, "Snow" },
            { 5001, "Flurries" },
            { 5100, "Light Snow" },
            { 5101, "Heavy Snow" },
            { 6000, "Freezing Drizzle" },
            { 6001, "Freezing Rain" },
            { 6200, "Light Freezing Rain" },
            { 6201, "Heavy Freezing Rain" },
            { 7000, "Ice Pellets" },
            { 7101, "Heavy Ice Pellets" },
            { 7102, "Light Ice Pellets" },
            { 8000, "Thunderstorm" }
        };

        public WeatherClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Weather> GetWeatherAsync(Coordinates coordinates, CancellationToken cancellationToken = default)
        {
            var weatherApi = $"https://api.tomorrow.io/v4/weather/realtime?location={coordinates.Latitude.ToString("N", CultureInfo.InvariantCulture)},{coordinates.Longitude.ToString("N", CultureInfo.InvariantCulture)}&units=metric&apikey={_configuration["TomorrowApi"]}".ToUri();
            var response = await weatherApi.HttpGetAsync(cancellationToken).ConfigureAwait(false);
            using (var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                var values = document.RootElement.GetProperty("data").GetProperty("values");
                return new Weather
                {
                    Condition = WeatherCodes[values.GetProperty("weatherCode").GetInt32()],
                    ConditionCode = values.GetProperty("weatherCode").GetInt32(),
                    Humidity = values.GetProperty("humidity").GetInt32(),
                    Temperature = values.GetProperty("temperature").GetDouble(),
                    TemperatureApparent = values.GetProperty("temperatureApparent").GetDouble(),
                    WindGust = values.GetProperty("windGust").GetDouble(),
                    WindSpeed =  values.GetProperty("windSpeed").GetDouble()
                };
            }
        }
    }
}
