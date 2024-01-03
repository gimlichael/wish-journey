using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalModified : IntegrationEvent
    {
        public JournalModified(Guid id, Guid ownerId, string title, string description)
        {
            Id = id;
            OwnerId = ownerId;
            Title = title;
            Description = description;
            Modified = DateTime.UtcNow;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTime Modified { get; }
    }
}
