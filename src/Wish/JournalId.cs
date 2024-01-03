using System;
using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record JournalId : SingleValueObject<Guid>
    {
        public JournalId() : base(Guid.NewGuid())
        {
        }

        public JournalId(string value) : this(Guid.Parse(value))
        {
        }

        public JournalId(Guid value) : base(Validator.CheckParameter(value, () => Validator.ThrowIfTrue(value == Guid.Empty, nameof(value))))
        {
        }

        public override string ToString() => Value.ToString("N");
	}
}
