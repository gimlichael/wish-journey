using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalInitiated : DomainEvent
    {
        public JournalInitiated(Journal journal)
        {
            Id = journal.Id;
            OwnerId = journal.OwnerId;
            Title = journal.Title;
            Description = journal.Description;
            Created = journal.Created;
        }

        public Guid Id { get; }

        public Guid OwnerId { get; }

        public string Title { get; }

        public string Description { get; }

        public DateTime Created { get; }
    }
}
