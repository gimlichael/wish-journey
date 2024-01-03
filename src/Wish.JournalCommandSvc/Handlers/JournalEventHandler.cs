using Microsoft.Extensions.Logging;
using Savvyio.EventDriven;
using Savvyio.EventDriven.Messaging;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Extensions.SimpleQueueService.EventDriven;
using Savvyio.Handlers;
using Wish.JournalApplication.Events;

namespace Wish.JournalCommandSvc.Handlers
{
    public class JournalEventHandler : IntegrationEventHandler
    {
	    private readonly ILogger<JournalEventHandler> _logger;
	    private readonly IPublishSubscribeChannel<IIntegrationEvent, JournalEventHandler> _eventBus;

        public JournalEventHandler(ILogger<JournalEventHandler> logger, IPublishSubscribeChannel<IIntegrationEvent, JournalEventHandler> eventBus)
        {
	        _logger = logger;
	        _eventBus = eventBus;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<IIntegrationEvent> handlers)
        {
            handlers.RegisterAsync<JournalCreated>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-event.fifo".ToSnsUri(), "journalcommandsvc.journals.created-event")));
            handlers.RegisterAsync<JournalModified>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-event.fifo".ToSnsUri(), "journalcommandsvc.journals.updated-event")));
            handlers.RegisterAsync<JournalDeleted>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-event.fifo".ToSnsUri(), "journalcommandsvc.journals.deleted-event")));
            handlers.RegisterAsync<JournalEntryCreated>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-entry-event.fifo".ToSnsUri(), "journalcommandsvc.journals.entries.created-event")));
            handlers.RegisterAsync<JournalEntryModified>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-entry-event.fifo".ToSnsUri(), "journalcommandsvc.journals.entries.updated-event")));
            handlers.RegisterAsync<JournalEntryDeleted>(@event => _eventBus.PublishAsync(@event.ToMessage("journal-entry-event.fifo".ToSnsUri(), "journalcommandsvc.journals.entries.deleted-event")));
        }
    }
}
