using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record TimezoneOffset : SingleValueObject<double>
    {
        public TimezoneOffset(double value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfLowerThan(value, -14, nameof(value));
            Validator.ThrowIfGreaterThan(value, 14, nameof(value));
        }))
        {
        }
    }
}
