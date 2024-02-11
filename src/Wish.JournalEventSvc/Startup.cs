using System;
using Amazon;
using Amazon.Runtime;
using Codebelt.Bootstrapper.Worker;
using Cuemon.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.Commands;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.EventDriven;
using Savvyio.Extensions.SimpleQueueService;
using Savvyio.Extensions.Text.Json;
using Wish.JournalAzureTableStorage;
using Wish.JournalEventSvc.Handlers;
using Wish.Shared;

namespace Wish.JournalEventSvc
{
    public class Startup : WorkerStartup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
	        AmazonResourceNameOptions.DefaultAccountId = configuration["AWS:CallerIdentity"];
        }

        public override void ConfigureServices(IServiceCollection services)
        {
	        services.AddApplicationInsightsTelemetryWorkerService(o => o.ConnectionString = Configuration["Azure:ApplicationInsights:ConnectionString"]);

	        services.AddSavvyIO(o =>
	        {
		        o.EnableHandlerServicesDescriptor()
                    .UseAutomaticDispatcherDiscovery()
                    .UseAutomaticHandlerDiscovery()
			        .AddMediator<Mediator>();
	        });

	        services.Add<JsonMarshaller>(o => o.Lifetime = ServiceLifetime.Singleton);

	        services.ConfigureTriple<AmazonEventBusOptions<JournalEventHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/wish-journal-event.fifo");
	        });
	        services.Add<AmazonEventBus<JournalEventHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);

	        services.ConfigureTriple<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/wish-journal-status.fifo");
	        });
	        services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);

	        services.Add<JournalDataStore>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<JournalTableOptions>()
                .ConfigureTriple(o => o.ConnectionString = Configuration.GetConnectionString("JournalTable"));
	        services.Add<JournalEntryDataStore>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<JournalTableOptions>()
                .ConfigureTriple(o => o.ConnectionString = Configuration.GetConnectionString("JournalTable"));

	        services.AddHostedService<JournalEventWorker>();
        }
    }
}
