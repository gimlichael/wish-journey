using Cuemon;
using Cuemon.Configuration;

namespace Wish.IpGeolocationService
{
    public class TimeZoneClientOptions : IValidatableParameterObject
    {
        public TimeZoneClientOptions()
        {
        }

        public string ApiKey { get; set; }

        public void ValidateOptions()
        {
            Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(ApiKey));
        }
    }
}
