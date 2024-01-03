using Savvyio.Domain;
using System;

namespace Wish
{
    public interface IJournalEntryRepository : IPersistentRepository<JournalEntry, Guid>
    {
    }
}
