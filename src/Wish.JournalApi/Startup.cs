using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Asp.Versioning;
using Codebelt.Bootstrapper.Web;
using Cuemon.AspNetCore.Diagnostics;
using Cuemon.Collections.Generic;
using Cuemon.Diagnostics;
using Cuemon.Extensions.Asp.Versioning;
using Cuemon.Extensions.AspNetCore.Authentication;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Cuemon.Extensions.AspNetCore.Hosting;
using Cuemon.Extensions.AspNetCore.Http.Headers;
using Cuemon.Extensions.AspNetCore.Mvc.Filters;
using Cuemon.Extensions.AspNetCore.Mvc.Formatters.Text.Json;
using Cuemon.Extensions.DependencyInjection;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Runtime.Caching;
using Cuemon.Extensions.Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.Extensions;
using Savvyio.Extensions.DependencyInjection;
using Savvyio.Extensions.DependencyInjection.SimpleQueueService.Commands;
using Savvyio.Extensions.Text.Json;
using Wish.JournalApi.Handlers;
using Wish.JournalAzureSqlServer;
using Wish.JournalAzureTableStorage;
using Wish.Shared;

namespace Wish.JournalApi
{
	public class Startup : WebStartup
	{
		public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddApplicationInsightsTelemetry(o => o.ConnectionString = Configuration["Azure:ApplicationInsights:ConnectionString"]);

			services.AddControllers(o =>
			{
				o.Filters.AddServerTiming();
				o.Filters.AddFaultDescriptor();
			}).AddJsonFormatters(o =>
			{
				o.SensitivityDetails = Environment.IsNonProduction() ? FaultSensitivityDetails.All : FaultSensitivityDetails.None;
			});

			services.AddRestfulApiVersioning(o =>
			{
				o.Conventions.Controller<Controllers.V1.JournalsController>().HasApiVersion(new ApiVersion(1, 0));
				o.Conventions.Controller<Controllers.V1.StatusController>().HasApiVersion(new ApiVersion(1, 0));
			});

			services.AddRestfulSwagger(o =>
			{
				o.OpenApiInfo.Title = "WISH Journey API";
				o.OpenApiInfo.Description = "An API to track GPS coordinates with point-in-time data relevant for the location. Think about Captains Log from Star Trek.";
				o.Settings.UseAllOfToExtendReferenceSchemas();
				o.Settings.AddXApiKeySecurity();
				o.Settings.AddBasicAuthenticationSecurity();
			});

			services.AddServerTiming();
			services.AddHttpContextAccessor();
			services.AddHttpClient();

			services.Configure<FaultDescriptorOptions>(o =>
			{
				o.SensitivityDetails = Environment.IsNonProduction() ? FaultSensitivityDetails.All : FaultSensitivityDetails.None;
			});

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

			services.Add<JournalDataSource>(o => o.Lifetime = ServiceLifetime.Scoped);
			services.Add<OwnerRepository>(o => o.Lifetime = ServiceLifetime.Scoped);
			services.Add<JournalDataStore>(o => o.Lifetime = ServiceLifetime.Scoped);
			services.Add<JournalEntryDataStore>(o => o.Lifetime = ServiceLifetime.Scoped);
			services.Add<StatusDataStore>(o => o.Lifetime = ServiceLifetime.Scoped);
		}

		public override void Configure(IApplicationBuilder app, ILogger logger)
		{
			logger.LogInformation("{registeredHandlers}", app.ApplicationServices.GetService<HandlerServicesDescriptor>());

			TimeMeasure.CompletedCallback = profiler => logger.LogTrace(profiler.ToString());

			app.UseFaultDescriptorExceptionHandler(o =>
			{
				o.ExceptionCallback = (_, exception, descriptor) =>
				{
					logger.LogError(exception, descriptor.ToString());
				};
				o.NonMvcResponseHandlers.AddYamlResponseHandler();
			});

			if (Environment.IsNonProduction())
			{
				if (Environment.IsLocalDevelopment())
				{
					app.UseDeveloperExceptionPage();
				}
			}
			else
			{
				app.UseHsts();
				app.UseHttpsRedirection();
			}

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseHostingEnvironment(o => o.SuppressHeaderPredicate = e => e.IsProduction());

			app.UseCors(builder =>
			{
				builder.AllowAnyHeader();
				builder.AllowAnyMethod();
				builder.AllowAnyOrigin();
			});

			app.UseRouting();

			app.UseApiKeySentinel(o => o.AllowedKeys.Add(Configuration["XApiKey"]));

			var json = Configuration["BasicAuthentication"]!;
			var credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
			var ownerRepository = GlobalCaching.Cache.Memoize(TimeSpan.FromHours(8), new Func<string, Guid?>(username =>
			{
				using (var scope = app.ApplicationServices.CreateScope())
				{
					return scope.ServiceProvider.GetRequiredService<IOwnerRepository>().FindAllAsync(owner => owner.EmailAddress == username).SingleOrDefaultAsync().GetAwaiter().GetResult()?.Id;
				}
			}));

			app.UseBasicAuthentication(o =>
			{
				o.RequireSecureConnection = false;
				o.Authenticator = (username, password) =>
				{
					if (credentials.Any(pair => pair.Key == username && pair.Value == password))
					{
						var ownerId = ownerRepository(username);
						if (ownerId.HasValue)
						{
							logger.LogInformation("Successful authentication for '{username}'.", username);
							return new ClaimsPrincipal(new ClaimsIdentity(Arguments.Yield(new Claim("OwnerId", ownerId.Value.ToString("N"), ClaimValueTypes.String))));
						}
					}
					logger.LogWarning("Failed authentication attempt for '{username}'.", username);
					return null;
				};
			});

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
