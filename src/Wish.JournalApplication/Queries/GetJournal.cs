using System;
using Savvyio.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
    public record GetJournal : Query<JournalViewModel>
    {
        public GetJournal(string ownerId, string journalId)
        {
            OwnerId = Guid.Parse(ownerId);
            JournalId = Guid.Parse(journalId);
        }

        public Guid OwnerId { get; }

        public Guid JournalId { get; }
    }
}
