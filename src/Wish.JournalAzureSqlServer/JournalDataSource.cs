using Cuemon;
using Microsoft.EntityFrameworkCore;
using Savvyio.Extensions.EFCore;

namespace Wish.JournalAzureSqlServer
{
    public class JournalDataSource : EfCoreDataSource
    {
        public JournalDataSource(JournalDataSourceOptions options) : base(new EfCoreDataSourceOptions
        {
            ContextConfigurator = b => b.UseSqlServer(Validator.CheckParameter(() =>
            {
                Validator.ThrowIfInvalidOptions(options);
                return options.ConnectionString;
            })),
            ModelConstructor = mb => mb.AddJournal()
        })
        {
        }
    }
}
