using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalEntryChanged : DomainEvent
    {
        public JournalEntryChanged(JournalEntry entry)
        {
            Id = entry.Id;
            JournalId = entry.JournalId;
            Notes = entry.Notes;
            Modified = entry.Modified.Value;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public string Notes { get; init; }

        public DateTimeOffset Modified { get; init; }
    }
}
