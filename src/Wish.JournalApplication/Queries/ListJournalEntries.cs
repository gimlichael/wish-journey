using System;
using System.Collections.Generic;
using Savvyio.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
    public record ListJournalEntries : Query<IEnumerable<JournalEntryCollectionViewModel>>
    {
        public ListJournalEntries(string ownerId, string journalId, int maxInclusiveResultCount)
        {
            OwnerId = Guid.Parse(ownerId);
            JournalId = Guid.Parse(journalId);
            MaxInclusiveResultCount = maxInclusiveResultCount;
        }

        public Guid OwnerId { get; }

        public Guid JournalId { get; }

        public int MaxInclusiveResultCount { get; }
    }
}
