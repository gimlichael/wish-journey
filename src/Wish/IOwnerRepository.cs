using Savvyio.Domain;
using System;

namespace Wish
{
    public interface IOwnerRepository : IPersistentRepository<Owner, Guid>
    {
    }
}
