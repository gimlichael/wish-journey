using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalEntryDeleted : IntegrationEvent
    {
        public JournalEntryDeleted(Guid id, Guid journalId, Guid ownerId)
        {
            Id = id;
            JournalId = journalId;
            OwnerId = ownerId;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public Guid JournalId { get; }
    }
}
