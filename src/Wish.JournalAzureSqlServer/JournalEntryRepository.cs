using System;
using Savvyio.Extensions.EFCore;

namespace Wish.JournalAzureSqlServer
{
    public class JournalEntryRepository : EfCoreRepository<JournalEntry, Guid>, IJournalEntryRepository
    {
        public JournalEntryRepository(IEfCoreDataSource source) : base(source)
        {
        }
    }
}
