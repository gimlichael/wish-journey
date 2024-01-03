using System;
using Savvyio.Domain;

namespace Wish
{
    public record Timestamp : ValueObject
    {
	    public static implicit operator DateTimeOffset(Timestamp value) => value.Value;

        public Timestamp(DateTimeOffset value)
        {
			Value = value;
		}

        public DateTimeOffset Value { get; }

        public string TimeZone { get; set; }

        public bool IsDaylightSavingTime { get; set; }
    }
}
