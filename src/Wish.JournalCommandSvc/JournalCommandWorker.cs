using System;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Bootstrapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio.Commands;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Wish.JournalCommandSvc.Handlers;

namespace Wish.JournalCommandSvc
{
	public class JournalCommandWorker : BackgroundService
	{
		private bool _applicationStopping = false;
		private readonly IMediator _mediator;
		private readonly IPointToPointChannel<ICommand, JournalCommandHandler> _commandQueue;
		private readonly ILogger<JournalCommandWorker> _logger;

		public JournalCommandWorker(IMediator mediator, IPointToPointChannel<ICommand, JournalCommandHandler> commandQueue, ILogger<JournalCommandWorker> logger)
		{
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
					var messages = await _commandQueue.ReceiveAsync().ConfigureAwait(false);
					foreach (var message in messages)
					{
						_logger.LogInformation("Processing: {message}", message);
						await _mediator.CommitAsync(message.Data).ConfigureAwait(false);
					}
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
