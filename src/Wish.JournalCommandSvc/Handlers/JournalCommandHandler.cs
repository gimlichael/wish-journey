﻿using System;
using System.Threading.Tasks;
using Cuemon.Diagnostics;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Domain;
using Savvyio.Extensions;
using Savvyio.Handlers;
using Wish.JournalApplication.Commands;
using Wish.JournalApplication.Events;
using Wish.JournalApplication.Projections;
using Wish.Services;

namespace Wish.JournalCommandSvc.Handlers
{
    public class JournalCommandHandler : CommandHandler
    {
        private readonly IMediator _mediator;
        private readonly IJournalRepository _journalRepository;
        private readonly IJournalEntryRepository _journalEntryRepository;
        private readonly IUnitOfWork _uow;
        private readonly ILocationService _locationService;
        private readonly IWeatherService _weatherService;
        private readonly ITimestampService _timestampService;
        private readonly ILogger<JournalCommandHandler> _logger;

        public JournalCommandHandler(IMediator mediator, 
            IJournalRepository journalRepository, 
            IJournalEntryRepository journalEntryRepository, 
            IUnitOfWork uow, 
            ILocationService locationService, 
            IWeatherService weatherService, 
            ITimestampService timestampService, 
            ILogger<JournalCommandHandler> logger)
        {
            _mediator = mediator;
            _journalRepository = journalRepository;
            _journalEntryRepository = journalEntryRepository;
            _uow = uow;
            _locationService = locationService;
            _weatherService = weatherService;
            _timestampService = timestampService;
            _logger = logger;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
        {
            handlers.RegisterAsync<CreateJournal>(CreateJournalAsync);
            handlers.RegisterAsync<UpdateJournal>(UpdateJournalAsync);
            handlers.RegisterAsync<DeleteJournal>(DeleteJournalAsync);
            handlers.RegisterAsync<CreateJournalEntry>(CreateJournalEntryAsync);
            handlers.RegisterAsync<UpdateJournalEntry>(UpdateJournalEntryAsync);
            handlers.RegisterAsync<DeleteJournalEntry>(DeleteJournalEntryAsync);
        }

        private async Task DeleteJournalEntryAsync(DeleteJournalEntry command)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var entity = await _journalRepository.GetByIdAsync(command.JournalId);
                    var entry = entity.RemoveEntry(command.EntryId);
                    _journalEntryRepository.Remove(entry);
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalEntryDeleted(command.EntryId, command.JournalId, command.OwnerId).MergeMetadata(command)).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Deleted journal entry {EntryId} from journal {JournalId}. Operation took {Duration}.", command.EntryId, command.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
	            {
		            Action = StatusAction.Delete,
		            Modified = DateTime.UtcNow,
		            Result = StatusResult.Failed,
		            Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

	            throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task UpdateJournalEntryAsync(UpdateJournalEntry command)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var entity = await _journalRepository.GetByIdAsync(command.JournalId);
                    var entry = entity.ChangeEntry(command.EntryId, command.Notes, ianaTimeZoneName => _timestampService.GetTimestampAsync(ianaTimeZoneName).GetAwaiter().GetResult());
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalEntryModified(entry.Id, entry.JournalId, entity.OwnerId, entry.Notes, entry.Modified!.Value).MergeMetadata(command)).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Updated journal entry {EntryId} in journal {JournalId}. Operation took {Duration}.", command.EntryId, command.JournalId, duration);
            }
            catch (Exception ex)
            {
                await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task CreateJournalEntryAsync(CreateJournalEntry command)
        {
            TimeSpan duration = TimeSpan.Zero;
            Guid entryId = Guid.Empty;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var timestamp = await _timestampService.GetTimestampAsync(command.Coordinates).ConfigureAwait(false);
                    var journal = await _journalRepository.GetByIdAsync(command.JournalId).ConfigureAwait(false);
                    var location = await _locationService.GetLocationAsync(command.Coordinates).ConfigureAwait(false);
                    var weather = await _weatherService.GetWeatherAsync(command.Coordinates).ConfigureAwait(false);
                    if (command.Timestamp != null)
                    {
                        command.Timestamp.TimeZone = timestamp.TimeZone;
                        command.Timestamp.IsDaylightSavingTime = timestamp.IsDaylightSavingTime;
                    }
                    var entity = journal.AddEntry(command.Coordinates, location, weather, command.Timestamp ?? timestamp, command.Notes);
                    _journalEntryRepository.Add(entity);
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalEntryCreated(entity.Id, 
                        entity.JournalId,
                        journal.OwnerId,
                        command.Coordinates.Latitude, 
                        command.Coordinates.Longitude, 
                        location.City, 
                        location.Country, 
                        location.Query, 
                        weather.Temperature, 
                        weather.TemperatureApparent, 
                        weather.Condition,
                        weather.ConditionCode,
                        weather.WindGust, 
                        weather.WindSpeed, 
                        weather.Humidity, 
                        command.Notes, 
                        command.Timestamp ?? timestamp).MergeMetadata(command)).ConfigureAwait(false);
                    entryId = entity.Id;
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Created journal entry {EntryId} in journal {JournalId}. Operation took {Duration}.", entryId, command.JournalId, duration);
            }
            catch (Exception ex)
            {
                await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task DeleteJournalAsync(DeleteJournal command)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var entity = await _journalRepository.GetByIdAsync(command.JournalId);
                    _journalRepository.Remove(entity);
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalDeleted(entity.Id, entity.OwnerId).MergeMetadata(command)).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Deleted journal {JournalId}. Operation took {Duration}.", command.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
                {
                    Action = StatusAction.Delete,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task UpdateJournalAsync(UpdateJournal command)
        {
            TimeSpan duration = TimeSpan.Zero;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var entity = await _journalRepository.GetByIdAsync(command.JournalId);
                    entity.ChangeDescription(command.Description);
                    entity.ChangeTitle(command.Title);
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalModified(entity.Id, entity.OwnerId, entity.Title, entity.Description).MergeMetadata(command)).ConfigureAwait(false);
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Updated journal {JournalId}. Operation took {Duration}.", command.JournalId, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
                {
                    Action = StatusAction.Update,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }

        private async Task CreateJournalAsync(CreateJournal command)
        {
            TimeSpan duration = TimeSpan.Zero;
            Guid id = Guid.Empty;
            try
            {
                var profiler = await TimeMeasure.WithActionAsync(async _ =>
                {
                    var entity = new Journal(command.OwnerId, command.Title, command.Description);
                    _journalRepository.Add(entity);
                    await _uow.SaveChangesAsync().ConfigureAwait(false);
                    await _mediator.PublishAsync(new JournalCreated(entity.Id, entity.OwnerId, entity.Title, entity.Description, entity.Created).MergeMetadata(command)).ConfigureAwait(false);
                    id = entity.Id;
                }).ConfigureAwait(false);
                duration = profiler.Elapsed;

                _logger.LogInformation("Created journal {JournalId}. Operation took {Duration}.", id, duration);
            }
            catch (Exception ex)
            {
	            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
                {
                    Action = StatusAction.Create,
                    Modified = DateTime.UtcNow,
                    Result = StatusResult.Failed,
                    Message = ex.ToString(),
                    Duration = duration
                }).ConfigureAwait(false);

                throw;
            }

            await _mediator.CommitAsync(new UpdateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Modified = DateTime.UtcNow,
                Result = StatusResult.Progressing,
                Duration = duration
            }).ConfigureAwait(false);
        }
    }
}
