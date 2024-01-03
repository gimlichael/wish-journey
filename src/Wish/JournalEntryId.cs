using System;
using Cuemon;
using Savvyio.Domain;

namespace Wish
{
    public record JournalEntryId : SingleValueObject<Guid>
    {
        public JournalEntryId() : base(Guid.NewGuid())
        {
        }

        public JournalEntryId(string value) : this(Guid.Parse(value))
        {
        }

        public JournalEntryId(Guid value) : base(Validator.CheckParameter(value, () => Validator.ThrowIfTrue(value == Guid.Empty, nameof(value))))
        {
        }

        public override string ToString() => Value.ToString("N");
    }
}
