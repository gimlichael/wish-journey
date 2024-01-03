using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record Title : SingleValueObject<string>
    {
        public Title(string value) : base(Validator.CheckParameter(value, () =>
        {
            Validator.ThrowIfNullOrWhitespace(value);
            Validator.ThrowIfGreaterThan(value.Length, 256, nameof(value));
        }))
        {
        }
    }
}
