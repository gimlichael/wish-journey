using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record Notes : SingleValueObject<string>
    {
        public Notes(string value) : base(Validator.CheckParameter(value, () =>
        {
            if (value == null) { return; }
            Validator.ThrowIfGreaterThan(value.Length, 2048, nameof(value));
        }))
        {
        }
    }
}
