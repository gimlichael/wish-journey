using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record EmailAddress : SingleValueObject<string>
    {
        public EmailAddress(string value) : base(Validator.CheckParameter(() =>
        {
            Validator.ThrowIfNullOrWhitespace(value);
            Validator.ThrowIfNotEmailAddress(value);
            return value.ToLowerInvariant();
        }))
        {
        }
    }
}
