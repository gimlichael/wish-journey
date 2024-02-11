using Cuemon;
using Cuemon.Configuration;

namespace Wish.LocationGeocodeService
{
    public class GeocodeClientOptions : IValidatableParameterObject
    {
        public GeocodeClientOptions()
        {
        }

        public string ApiKey { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ApiKey));
        }
    }
}
