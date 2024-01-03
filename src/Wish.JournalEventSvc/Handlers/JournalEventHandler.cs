using System;
using System.Threading.Tasks;
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

        public JournalEventHandler(IMediator mediator, IJournalDataStore journalDataStore, IJournalEntryDataStore journalEntryDataStore)
        {
	        _mediator = mediator;
            _journalDataStore = journalDataStore;
            _journalEntryDataStore = journalEntryDataStore;
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
            var projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId);

            try
            {
                await _journalEntryDataStore.DeleteAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Delete,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed
            }).ConfigureAwait(false);
        }

        private async Task OnJournalEntryModifiedAsync(JournalEntryModified @event)
        {
            var projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId)
            {
                Modified = @event.Modified,
                Notes = @event.Notes
            };

            try
            {
                await _journalEntryDataStore.UpdateAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed
            }).ConfigureAwait(false);
        }

        private async Task OnJournalDeletedAsync(JournalDeleted @event)
        {
            var projection = new JournalProjection(@event.Id, @event.OwnerId);

            try
            {
                await _journalDataStore.DeleteAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Delete,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed
            }).ConfigureAwait(false);
        }

        private async Task OnJournalEntryCreatedAsync(JournalEntryCreated @event)
        {
            var projection = new JournalEntryProjection(@event.Id, @event.JournalId, @event.OwnerId)
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

            try
            {
                await _journalEntryDataStore.CreateAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                EndpointRouteValue = $"/{projection.Id:N}"
            }).ConfigureAwait(false);
        }

        private async Task OnJournalModifiedAsync(JournalModified @event)
        {
            var projection = new JournalProjection(@event.Id, @event.OwnerId)
            {
                Modified = @event.Modified,
                Description = @event.Description,
                Title = @event.Title
            };

            try
            {
                await _journalDataStore.UpdateAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed
            }).ConfigureAwait(false);
        }

        private async Task OnJournalCreatedAsync(JournalCreated @event)
        {
            var projection = new JournalProjection(@event.Id, @event.OwnerId)
            {
                Created = @event.Created,
                Description = @event.Description,
                Title = @event.Title
            };

            try
            {
                await _journalDataStore.CreateAsync(projection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.Message
                }).ConfigureAwait(false);
                
	            throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(new OwnerId(@event.OwnerId), @event.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Completed,
                EndpointRouteValue = $"/{projection.Id:N}"
            }).ConfigureAwait(false);
        }
    }
}
