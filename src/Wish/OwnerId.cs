using System;
using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record OwnerId : SingleValueObject<Guid>
    {
        public OwnerId() : base(Guid.NewGuid())
        {
        }

        public OwnerId(string value) : this(Guid.Parse(value))
        {
        }

        public OwnerId(Guid value) : base(Validator.CheckParameter(value, () => Validator.ThrowIfTrue(value == Guid.Empty, nameof(value))))
        {
        }
    }
}
