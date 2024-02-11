using System;
using System.Threading;
using System.Threading.Tasks;
using Codebelt.Bootstrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.EventDriven;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Wish.JournalEventSvc.Handlers;

namespace Wish.JournalEventSvc
{
    public class JournalEventWorker : BackgroundService
    {
        private bool _applicationStopping = false;
        private readonly ILogger<JournalEventWorker> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IServiceScopeFactory _scopeFactory;

        public JournalEventWorker(ILogger<JournalEventWorker> logger, IServiceProvider serviceProvider, IHostEnvironment environment, IServiceScopeFactory scopeFactory)
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
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_applicationStopping) { return; }

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var eventBus = scope.ServiceProvider.GetService<IPublishSubscribeChannel<IIntegrationEvent, JournalEventHandler>>();
                    await eventBus.SubscribeAsync((message, _) =>
                    {
                        _logger.LogInformation("Processing: {message}", message);
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        return mediator.PublishAsync(message.Data);
                    }).ConfigureAwait(false);
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
