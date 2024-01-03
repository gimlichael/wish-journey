using System;
using System.Threading.Tasks;
using Cuemon.Threading;
using Savvyio.Domain;

namespace Wish
{
    public interface IJournalRepository : IPersistentRepository<Journal, Guid>
    {
        public new Task<Journal> GetByIdAsync(Guid id, Action<AsyncOptions> setup = null);
    }
}
