using Savvyio.Data;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication
{
    public interface IJournalDataStore : IPersistentDataStore<JournalProjection, QueryOptions<JournalProjection>>
    {
    }
}
