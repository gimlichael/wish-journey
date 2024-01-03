using System;

namespace Wish.JournalApplication.Inputs
{
    public record JournalEntryCreateInputModel
    {
        public JournalEntryCreateInputModel()
        {
        }

        public DateTimeOffset? Timestamp { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Notes { get; set; }
    }
}
