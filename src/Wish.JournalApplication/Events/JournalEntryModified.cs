using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalEntryModified : IntegrationEvent
    {
        public JournalEntryModified(Guid id, Guid journalId, Guid ownerId, string notes, DateTimeOffset modified)
        {
            Id = id;
            JournalId = journalId;
            OwnerId = ownerId;
            Notes = notes;
            Modified = modified;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public Guid OwnerId { get; }

        public string Notes { get; }

        public DateTimeOffset Modified { get; }
    }
}
