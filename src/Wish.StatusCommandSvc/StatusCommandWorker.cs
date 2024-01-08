using System;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.Commands;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Wish.StatusCommandSvc.Handlers;

namespace Wish.StatusCommandSvc
{
	public class StatusCommandWorker : BackgroundService
	{
		private bool _applicationStopping = false;
		private readonly IMediator _mediator;
		private readonly IPointToPointChannel<ICommand, StatusCommandHandler> _commandQueue;
		private readonly ILogger<StatusCommandWorker> _logger;

		public StatusCommandWorker(IMediator mediator, IPointToPointChannel<ICommand, StatusCommandHandler> commandQueue, ILogger<StatusCommandWorker> logger, IServiceProvider serviceProvider)
		{
			BootstrapperLifetime.OnApplicationStartedCallback = () =>
			{
				logger.LogInformation("{registeredHandlers}", serviceProvider.GetService<HandlerServicesDescriptor>());
			};
			BootstrapperLifetime.OnApplicationStoppingCallback = () =>
			{
				_applicationStopping = true;
			};
			_mediator = mediator;
			_commandQueue = commandQueue;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				if (_applicationStopping) { return; }

				try
				{
					await foreach (var message in _commandQueue.ReceiveAsync(o => o.CancellationToken = stoppingToken).ConfigureAwait(false))
					{
						_logger.LogInformation("Processing: {message}", message);
						await _mediator.CommitAsync(message.Data).ConfigureAwait(false);
					}
				}
				catch (Exception e)
				{
					_logger.LogError(e, e.Message);
				}

				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			}
		}
	}
}
