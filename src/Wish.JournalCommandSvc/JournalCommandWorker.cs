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
using Wish.JournalCommandSvc.Handlers;

namespace Wish.JournalCommandSvc
{
    public class JournalCommandWorker : BackgroundService
    {
        private bool _applicationStopping = false;
        private readonly ILogger<JournalCommandWorker> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IMediator _mediator;
        private readonly IPointToPointChannel<ICommand, JournalCommandHandler> _queue;

        public JournalCommandWorker(ILogger<JournalCommandWorker> logger, IServiceProvider serviceProvider, IHostEnvironment environment, IMediator mediator, IPointToPointChannel<ICommand, JournalCommandHandler> queue)
        {
            BootstrapperLifetime.OnApplicationStartedCallback = () =>
            {
                logger.LogInformation("{registeredHandlers}", serviceProvider.GetService<HandlerServicesDescriptor>());
            };
            BootstrapperLifetime.OnApplicationStoppingCallback = () =>
            {
                _applicationStopping = true;
            };
            _logger = logger;
            _environment = environment;
            _mediator = mediator;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_applicationStopping) { return; }

                try
                {
                    await foreach (var message in _queue.ReceiveAsync(o =>
                                   {
                                       o.CancellationToken = stoppingToken;
                                   }).ConfigureAwait(false))
                    {
                        _logger.LogInformation("Processing: {message}", message);
                        await _mediator.CommitAsync(message.Data).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }

                if (_environment.IsProduction())
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
        }
    }
}
