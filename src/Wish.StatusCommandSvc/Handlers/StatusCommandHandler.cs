using System.Threading.Tasks;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Handlers;
using Wish.JournalApplication;
using Wish.JournalApplication.Commands;
using Wish.JournalApplication.Projections;

namespace Wish.StatusCommandSvc.Handlers
{
    public class StatusCommandHandler : CommandHandler
    {
        private readonly IStatusDataStore _statusDataStore;

        public StatusCommandHandler(IStatusDataStore statusDataStore)
        {
            _statusDataStore = statusDataStore;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
        {
            handlers.RegisterAsync<CreateStatus>(CreateStatusAsync);
            handlers.RegisterAsync<UpdateStatus>(UpdateStatusAsync);
        }

        private async Task UpdateStatusAsync(UpdateStatus command)
        {
	        var projection = await _statusDataStore.FindAllAsync(o => o.Filter = dto => dto.CorrelationId == command.CorrelationId).SingleOrDefaultAsync().ConfigureAwait(false);
	        if (projection != null)
	        {
                projection.Action = command.Action;
				projection.Modified = command.Modified;
				projection.Result = command.Result;
				projection.Message = command.Message;
				if (command.EndpointRouteValue != null) { projection.EndpointRouteValue = command.EndpointRouteValue; }

                if (projection.DurationInTicks.HasValue)
                {
                    projection.DurationInTicks += command.Duration.Ticks;
                }
                else
                {
                    projection.DurationInTicks = command.Duration.Ticks;
                }
            }
	        await _statusDataStore.UpdateAsync(projection).ConfigureAwait(false);
        }

        private Task CreateStatusAsync(CreateStatus command)
        {
            var projection = new StatusProjection(command.OwnerId, command.CorrelationId)
            {
                Action = command.Action,
                Created = command.Created,
                Endpoint = command.Endpoint,
                Message = command.Message
            };
            return _statusDataStore.CreateAsync(projection);
        }
    }
}
