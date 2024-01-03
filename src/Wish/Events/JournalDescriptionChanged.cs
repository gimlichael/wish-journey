using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalDescriptionChanged : DomainEvent
    {
        public JournalDescriptionChanged(Journal journal)
        {
            Id = journal.Id;
            Description = journal.Description;
        }

        public Guid Id { get; init; }

        public string Description { get; init; }
    }
}
