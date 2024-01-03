using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalDeleted : IntegrationEvent
    {
        public JournalDeleted(Guid id, Guid ownerId)
        {
            Id = id;
            OwnerId = ownerId;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }
    }
}
