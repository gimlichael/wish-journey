using System;
using Savvyio.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
    public record GetJournalEntry : Query<JournalEntryViewModel>
    {
        public GetJournalEntry(string ownerId, string journalId, string entryId)
        {
			OwnerId = Guid.Parse(ownerId);
			JournalId = Guid.Parse(journalId);
            EntryId = Guid.Parse(entryId);
        }

        public Guid OwnerId { get; }

        public Guid JournalId { get; }

        public Guid EntryId { get; }
    }
}
