using System;
using System.Collections.Generic;
using Savvyio.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
    public record ListJournal : Query<IEnumerable<JournalCollectionViewModel>>
    {
        public ListJournal(string ownerId, int maxInclusiveResultCount)
        {
            OwnerId = Guid.Parse(ownerId);
            MaxInclusiveResultCount = maxInclusiveResultCount;
        }

        public Guid OwnerId { get; }

        public int MaxInclusiveResultCount { get; }
    }
}
