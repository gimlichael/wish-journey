using System;
using Savvyio.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
    public record GetJournalEntry : Query<JournalEntryViewModel>
    {
        public GetJournalEntry(string id, string entryId)
        {
            Id = Guid.Parse(id);
            EntryId = Guid.Parse(entryId);
        }

        public Guid Id { get; }

        public Guid EntryId { get; }
    }
}
