using System;
using System.Threading.Tasks;
using Cuemon.Diagnostics;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.EventDriven;
using Savvyio.Extensions;
using Savvyio.Handlers;
using Wish.JournalApplication;
using Wish.JournalApplication.Commands;
using Wish.JournalApplication.Events;
using Wish.JournalApplication.Projections;

namespace Wish.JournalEventSvc.Handlers
{
    public class JournalEventHandler : IntegrationEventHandler
    {
	    private readonly IMediator _mediator;
        private readonly IJournalDataStore _journalDataStore;
        private readonly IJournalEntryDataStore _journalEntryDataStore;
        private readonly ILogger<JournalEventHandler> _logger;

        public JournalEventHandler(IMediator mediator, 
            IJournalDataStore journalDataStore, 
            IJournalEntryDataStore journalEntryDataStore, 
            ILogger<JournalEventHandler> logger)
        {
	        _mediator = mediator;
            _journalDataStore = journalDataStore;
            _journalEntryDataStore = journalEntryDataStore;
            _logger = logger;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<IIntegrationEvent> handlers)
        {
            handlers.RegisterAsync<JournalCreated>(OnJournalCreatedAsync);
            handlers.RegisterAsync<JournalModified>(OnJournalModifiedAsync);
            handlers.RegisterAsync<JournalDeleted>(OnJournalDeletedAsync);
            handlers.RegisterAsync<JournalEntryCreated>(OnJournalEntryCreatedAsync);
            handlers.RegisterAsync<JournalEntryModified>(OnJournalEntryModifiedAsync);
            handlers.RegisterAsync<JournalEntryDeleted>(OnJournalEntryDeletedAsync);
        }

        private async Task OnJournalEntryDeletedAsync(JournalEntryDeleted @event)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId);
                    await _journalEntryDataStore.DeleteAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Deleted journal-projection entry {EntryId} from journal-projection {JournalId}. Operation took {Duration}.", @event.Id, @event.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Delete,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task OnJournalEntryModifiedAsync(JournalEntryModified @event)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId)
                    {
                        Modified = @event.Modified,
                        Notes = @event.Notes
                    };
                    await _journalEntryDataStore.UpdateAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Updated journal-projection entry {EntryId} in journal-projection {JournalId}. Operation took {Duration}.", @event.Id, @event.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task OnJournalDeletedAsync(JournalDeleted @event)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var projection = new JournalProjection(@event.Id, @event.OwnerId);
                    await _journalDataStore.DeleteAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Deleted journal-projection {JournalId}. Operation took {Duration}.", @event.Id, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Delete,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task OnJournalEntryCreatedAsync(JournalEntryCreated @event)
        {
            JournalEntryProjection projection = null;
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId)
                    {
                        Created = @event.Created,
                        CoordinatesLatitude = @event.CoordinatesLatitude,
                        CoordinatesLongitude = @event.CoordinatesLongitude,
                        LocationCity = @event.LocationCity,
                        LocationCountry = @event.LocationCountry,
                        LocationQuery = @event.LocationQuery,
                        Notes = @event.Notes,
                        WeatherCondition = @event.WeatherCondition,
                        WeatherConditionCode = @event.WeatherConditionCode,
                        WeatherHumidity = @event.WeatherHumidity,
                        WeatherTemperature = @event.WeatherTemperature,
                        WeatherTemperatureApparent = @event.WeatherTemperatureApparent,
                        WeatherWindGust = @event.WeatherWindGust,
                        WeatherWindSpeed = @event.WeatherWindSpeed
                    };
                    await _journalEntryDataStore.CreateAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Created journal-projection entry {EntryId} in journal-projection {JournalId}. Operation took {Duration}.", @event.Id, @event.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                EndpointRouteValue = $"/{projection.Id:N}",
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task OnJournalModifiedAsync(JournalModified @event)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var projection = new JournalProjection(@event.Id, @event.OwnerId)
                    {
                        Modified = @event.Modified,
                        Description = @event.Description,
                        Title = @event.Title
                    };
                    await _journalDataStore.UpdateAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Updated journal-projection {JournalId}. Operation took {Duration}.", @event.Id, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task OnJournalCreatedAsync(JournalCreated @event)
        {
            JournalProjection projection = null;
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    projection = new JournalProjection(@event.Id, @event.OwnerId)
                    {
                        Created = @event.Created,
                        Description = @event.Description,
                        Title = @event.Title
                    };
                    await _journalDataStore.CreateAsync(projection).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Created journal-projection {JournalId}. Operation took {Duration}.", @event.Id, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);
                
	            throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                EndpointRouteValue = $"/{projection.Id:N}",
                Duration = duration
            }).ConfigureAwait(false);
        }
    }
}
