using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalTitleChanged : DomainEvent
    {
        public JournalTitleChanged(Journal journal)
        {
            Id = journal.Id;
            Title = journal.Title;
        }

        public Guid Id { get; init; }

        public string Title { get; init; }
    }
}
