using System;
using System.Threading.Tasks;
using Savvyio;
using Savvyio.Handlers;
using Savvyio.Queries;
using Wish.JournalApplication;
using Wish.JournalApplication.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApi.Handlers
{
    public class StatusQueryHandler : QueryHandler
    {
	    private readonly IStatusDataStore _statusDataStore;

	    public StatusQueryHandler(IStatusDataStore statusDataStore)
	    {
		    _statusDataStore = statusDataStore;
	    }

        protected override void RegisterDelegates(IRequestReplyRegistry<IQuery> handlers)
        {
            handlers.RegisterAsync<GetStatus, StatusViewModel>(GetStatusAsync);
        }

        private async Task<StatusViewModel> GetStatusAsync(GetStatus query)
        {
	        var projection = await _statusDataStore.FindAllAsync(o => o.Filter = dto => dto.CorrelationId == query.CorrelationId).SingleOrDefaultAsync().ConfigureAwait(false);
	        return projection == null ? null : new StatusViewModel()
	        {
		        Created = projection.Created,
		        Modified = projection.Modified,
		        Action = projection.Action,
		        Endpoint = projection.Endpoint,
		        EndpointRouteValue = projection.EndpointRouteValue,
		        Message = projection.Message,
				Duration = projection.DurationInTicks.HasValue 
                    ? new TimeSpan(projection.DurationInTicks.Value) 
                    : null,
                Result = projection.Result
	        };
        }
    }
}
