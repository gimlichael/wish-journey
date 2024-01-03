using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalEntryAdded : DomainEvent
    {
        public JournalEntryAdded(JournalEntry entry)
        {
            Id = entry.Id;
            JournalId = entry.JournalId;
            Location = entry.Location;
            Weather = entry.Weather;
            Notes = entry.Notes;
            Created = entry.Created;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public Location Location { get; init; }

        public Weather Weather { get; init; }

        public string Notes { get; init; }

        public DateTimeOffset Created { get; init; }
    }
}
