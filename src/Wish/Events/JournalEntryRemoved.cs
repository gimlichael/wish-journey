using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalEntryRemoved : DomainEvent
    {
        public JournalEntryRemoved(JournalEntry entry)
        {
            Id = entry.Id;
            JournalId = entry.JournalId;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }
    }
}
