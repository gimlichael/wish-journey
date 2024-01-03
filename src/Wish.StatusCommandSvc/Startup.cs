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
using Savvyio.Extensions.Text.Json;
using Wish.JournalAzureTableStorage;
using Wish.StatusCommandSvc.Handlers;

namespace Wish.StatusCommandSvc
{
    public class Startup : WorkerStartup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
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

	        services.Configure<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
	        {
		        o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
		        o.Endpoint = RegionEndpoint.EUWest1;
		        o.SourceQueue = new Uri($"https://sqs.eu-west-1.amazonaws.com/{Configuration["AWS:CallerIdentity"]}/wish-journal-status.fifo");
	        });
	        services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Scoped);

	        services.Add<StatusDataStore>(o => o.Lifetime = ServiceLifetime.Scoped);

			services.AddHostedService<StatusCommandWorker>();
		}
    }
}
