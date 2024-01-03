using System;
using Savvyio.EventDriven;

namespace Wish.JournalApplication.Events
{
    public record JournalCreated : IntegrationEvent
    {
        public JournalCreated(Guid id, Guid ownerId, string title, string description, DateTime created)
        {
            Id = id;
            OwnerId = ownerId;
            Title = title;
            Description = description;
            Created = created;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTime Created { get; }
    }
}
