using System;
using Savvyio.Extensions.EFCore;

namespace Wish.JournalAzureSqlServer
{
    public class OwnerRepository : EfCoreRepository<Owner, Guid>, IOwnerRepository
    {
        public OwnerRepository(IEfCoreDataSource source) : base(source)
        {
        }
    }
}
