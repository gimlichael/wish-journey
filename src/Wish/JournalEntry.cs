using System;
using Savvyio.Domain;

namespace Wish
{
    public class JournalEntry : Entity<Guid>
    {
        JournalEntry()
        {
        }

        public JournalEntry(JournalEntryId id) : base(id)
        {
        }

        public JournalEntry(JournalId journalId, Coordinates coordinates, Location location, Weather weather, Timestamp timestamp, Notes notes = null)
        {
            Id = new JournalEntryId();
            JournalId = journalId;
            Coordinates = coordinates;
            Location = location;
            Weather = weather;
            Created = timestamp;
            TimeZone = timestamp.TimeZone;
            Notes = notes;
        }

        public Guid JournalId { get; private set; }

        public string TimeZone { get; private set; }

        public string Notes { get; set; }

        public Coordinates Coordinates { get; set; }

        public Location Location { get; set; }

        public Weather Weather { get; set; }

        public DateTimeOffset Created { get; }

        public DateTimeOffset? Modified { get; set; }
    }
}
