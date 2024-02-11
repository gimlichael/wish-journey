using System;
using Savvyio.Domain;

namespace Wish
{
    public interface IJournalRepository : IPersistentRepository<Journal, Guid>
    {
    }
}
