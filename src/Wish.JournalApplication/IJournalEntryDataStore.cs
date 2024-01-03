using Savvyio.Data;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication
{
    public interface IJournalEntryDataStore : IPersistentDataStore<JournalEntryProjection, QueryOptions<JournalEntryProjection>>
    {
    }
}
