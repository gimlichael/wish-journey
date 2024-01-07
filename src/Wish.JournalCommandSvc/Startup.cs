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
using Wish.IpGeolocationService;
using Wish.JournalAzureSqlServer;
using Wish.JournalCommandSvc.Handlers;
using Wish.LocationGeocodeService;
using Wish.WeatherTomorrowService;

namespace Wish.JournalCommandSvc
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

	        services.ConfigureTriple<AmazonCommandQueueOptions<JournalCommandHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"https://sqs.eu-west-1.amazonaws.com/{Configuration["AWS:CallerIdentity"]}/wish-journal-command.fifo");
	        });
	        services.Add<AmazonCommandQueue<JournalCommandHandler>>(o => o.Lifetime = ServiceLifetime.Scoped);

	        services.ConfigureTriple<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"https://sqs.eu-west-1.amazonaws.com/{Configuration["AWS:CallerIdentity"]}/wish-journal-status.fifo");
	        });
	        services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Scoped);

	        services.ConfigureTriple<AmazonEventBusOptions<JournalEventHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"https://sqs.eu-west-1.amazonaws.com/{Configuration["AWS:CallerIdentity"]}/wish-journal-event.fifo");
	        });
	        services.Add<AmazonEventBus<JournalEventHandler>>(o => o.Lifetime = ServiceLifetime.Scoped);

	        services.Add<JournalDataSource>(o => o.Lifetime = ServiceLifetime.Scoped);
	        services.Add<JournalRepository>(o => o.Lifetime = ServiceLifetime.Scoped);
	        services.Add<JournalEntryRepository>(o => o.Lifetime = ServiceLifetime.Scoped);
	        services.Add<GeocodeClient>(o => o.Lifetime = ServiceLifetime.Scoped);
	        services.Add<WeatherClient>(o => o.Lifetime = ServiceLifetime.Scoped);
	        services.Add<TimeZoneClient>(o => o.Lifetime = ServiceLifetime.Scoped);

	        services.AddHostedService<JournalCommandWorker>();
        }
    }
}
