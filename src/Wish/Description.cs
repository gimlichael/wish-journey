using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record Description : SingleValueObject<string>
    {
        public Description(string value) : base(Validator.CheckParameter(value, () =>
        {
            if (value == null) { return; }
            Validator.ThrowIfGreaterThan(value.Length, 1024, nameof(value));
        }))
        {
        }
    }
}
