using System;
using Savvyio.Domain;

namespace Wish.Events
{
    internal record JournalOwnerChanged : DomainEvent
    {
        public JournalOwnerChanged(Journal journal, Owner owner)
        {
            Id = owner.Id;
            JournalId = journal.Id;
            EmailAddress = owner.EmailAddress;
        }

        public Guid Id { get; }

        public Guid JournalId { get; }

        public string EmailAddress { get; }
    }
}
