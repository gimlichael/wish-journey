using System;
using System.Threading.Tasks;
using Cuemon.Extensions;
using Cuemon.Threading;
using Microsoft.EntityFrameworkCore;
using Savvyio.Extensions.EFCore;

namespace Wish.JournalAzureSqlServer
{
    public class JournalRepository : EfCoreRepository<Journal, Guid>, IJournalRepository
    {
        private readonly IEfCoreDataSource _source;

        public JournalRepository(IEfCoreDataSource source) : base(source)
        {
            _source = source;
        }

        public override Task<Journal> GetByIdAsync(Guid id, Action<AsyncOptions> setup = null)
        {
            var options = setup.Configure();
            return _source.Set<Journal>()
                .Include(j => j.Entries)
                .SingleOrDefaultAsync(j => j.Id == id, options.CancellationToken);
        }
    }
}
