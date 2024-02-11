using Cuemon;
using Cuemon.Configuration;

namespace Wish.WeatherTomorrowService
{
    public class WeatherClientOptions : IValidatableParameterObject
    {
        public WeatherClientOptions()
        {
        }

        public string ApiKey { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ApiKey));
        }
    }
}
