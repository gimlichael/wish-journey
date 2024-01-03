using Savvyio.Data;
using Wish.JournalApplication.Projections;

namespace Wish.JournalApplication
{
    public interface IStatusDataStore : IWritableDataStore<StatusProjection>, IDeletableDataStore<StatusProjection>, ISearchableDataStore<StatusProjection, QueryOptions<StatusProjection>>
    {
    }
}
