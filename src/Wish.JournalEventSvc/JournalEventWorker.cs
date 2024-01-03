using System;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Bootstrapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio.EventDriven;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Wish.JournalEventSvc.Handlers;

namespace Wish.JournalEventSvc
{
	public class JournalEventWorker : BackgroundService
	{
		private bool _applicationStopping = false;
		private readonly IMediator _mediator;
		private readonly IPublishSubscribeChannel<IIntegrationEvent, JournalEventHandler> _eventBus;
		private readonly ILogger<JournalEventWorker> _logger;

		public JournalEventWorker(IMediator mediator, IPublishSubscribeChannel<IIntegrationEvent, JournalEventHandler> eventBus, ILogger<JournalEventWorker> logger)
		{
			BootstrapperLifetime.OnApplicationStoppingCallback = () =>
			{
				_applicationStopping = true;
			};
			_mediator = mediator;
			_eventBus = eventBus;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				if (_applicationStopping) { return; }

				try
				{
					await _eventBus.SubscribeAsync((message, _) =>
					{
						_logger.LogInformation("Processing: {message}", message);
						return _mediator.PublishAsync(message.Data);
					}).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					_logger.LogError(e, e.Message);
				}

				await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
			}
		}
	}
}
