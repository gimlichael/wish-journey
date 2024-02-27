using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Amazon;
using Amazon.Runtime;
using Asp.Versioning;
using Codebelt.Bootstrapper.Web;
using Cuemon.AspNetCore.Authentication.Basic;
using Cuemon.Collections.Generic;
using Cuemon.Diagnostics;
using Cuemon.Extensions;
using Cuemon.Extensions.Asp.Versioning;
using Cuemon.Extensions.AspNetCore.Authentication;
using Cuemon.Extensions.AspNetCore.Diagnostics;
using Cuemon.Extensions.AspNetCore.Hosting;
using Cuemon.Extensions.AspNetCore.Mvc.Filters;
using Cuemon.Extensions.AspNetCore.Mvc.Filters.Cacheable;
using Cuemon.Extensions.AspNetCore.Mvc.Formatters.Text.Json;
using Cuemon.Extensions.DependencyInjection;
using Cuemon.Extensions.Hosting;
using Cuemon.Extensions.Runtime.Caching;
using Cuemon.Extensions.Swashbuckle.AspNetCore;
using Cuemon.Runtime.Caching;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
		private ILogger _logger;
		private IDictionary<string, string> _credentials;
		private Func<string, Guid?> _ownerRepositoryCallback;

		public Startup(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
		{
		}

		public override void ConfigureServices(IServiceCollection services)
		{
			services.AddApplicationInsightsTelemetry(o => o.ConnectionString = Configuration["Azure:ApplicationInsights:ConnectionString"]);

			services
                .AddRouting(o => o.LowercaseUrls = true)
				.AddControllers(o =>
				{
					o.Filters.AddServerTiming();
					o.Filters.AddFaultDescriptor();
					o.Filters.AddApiKeySentinel();
					o.Filters.AddHttpCacheable();
				})
				.AddJsonFormatters()
				.AddFaultDescriptorOptions()
				.AddApiKeySentinelOptions(o => o.AllowedKeys.Add(Configuration["XApiKey"]))
				.AddHttpCacheableOptions(o =>
				{
					o.Filters.AddEntityTagHeader(fo => fo.UseEntityTagResponseParser = true);
				});

			services
				.AddAuthentication(BasicAuthorizationHeader.Scheme)
				.AddBasic(o =>
				{
					o.RequireSecureConnection = false;
					o.Authenticator = (username, password) =>
					{
						if (_credentials.Any(pair => pair.Key == username && pair.Value == password))
						{
							var ownerId = _ownerRepositoryCallback(username);
							if (ownerId.HasValue)
							{
								_logger.LogInformation("Successful authentication for '{username}'.", username);
								return new ClaimsPrincipal(new ClaimsIdentity(Arguments.Yield(new Claim("OwnerId", ownerId.Value.ToString("N"), ClaimValueTypes.String)), BasicAuthorizationHeader.Scheme));
							}
						}
						_logger.LogWarning("Failed authentication attempt for '{username}'.", username);
						return null;
					};
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

			services
				.AddServerTiming(o => o.UseTimeMeasureProfiler = true)
				.AddHttpContextAccessor()
				.AddHttpClient()
				.AddFaultDescriptorOptions(o =>
				{
					o.ExceptionCallback = (_, exception, descriptor) =>
					{
						_logger.LogError(exception, descriptor.ToString());
					};
				})
				.AddAuthorizationResponseHandler()
				.AddSavvyIO(o =>
				{
					o.EnableHandlerServicesDescriptor()
						.UseAutomaticDispatcherDiscovery()
						.UseAutomaticHandlerDiscovery()
						.AddMediator<Mediator>();
				})
				.PostConfigureAllOf<IExceptionDescriptorOptions>(o => o.SensitivityDetails = Environment.IsNonProduction() ? FaultSensitivityDetails.All : FaultSensitivityDetails.None);

            services.ConfigureTriple<AmazonCommandQueueOptions<JournalCommandHandler>>(o =>
            {
                o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
                o.Endpoint = RegionEndpoint.EUWest1;
                o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/wish-journal-command.fifo");
            });


            services.ConfigureTriple<AmazonCommandQueueOptions<StatusCommandHandler>>(o =>
            {
                o.Credentials = new BasicAWSCredentials(Configuration["AWS:IAM:AccessKey"], Configuration["AWS:IAM:SecretKey"]);
                o.Endpoint = RegionEndpoint.EUWest1;
                o.SourceQueue = new Uri($"{Configuration["AWS:SourceQueue"]}/{Configuration["AWS:CallerIdentity"]}/wish-journal-status.fifo");
            });

			services.Add<JsonMarshaller>(o => o.Lifetime = ServiceLifetime.Singleton);
			services.Add<AmazonCommandQueue<JournalCommandHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);
			services.Add<AmazonCommandQueue<StatusCommandHandler>>(o => o.Lifetime = ServiceLifetime.Singleton);
			services.Add<JournalDataSource>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<JournalDataSourceOptions>()
                .ConfigureTriple(o =>
                {
                    o.ConnectionString = Configuration.GetConnectionString("Journal");
                });
			services.Add<OwnerRepository>(o => o.Lifetime = ServiceLifetime.Scoped);
			services.Add<JournalDataStore>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<JournalTableOptions>()
                .ConfigureTriple(o => o.ConnectionString = Configuration.GetConnectionString("JournalTable"));
			services.Add<JournalEntryDataStore>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<JournalTableOptions>()
                .ConfigureTriple(o => o.ConnectionString = Configuration.GetConnectionString("JournalTable"));
			services.Add<StatusDataStore>(o => o.Lifetime = ServiceLifetime.Scoped)
                .AddOptions<StatusTableOptions>()
                .ConfigureTriple(o => o.ConnectionString = Configuration.GetConnectionString("JournalTable"));
		}

		public override void Configure(IApplicationBuilder app, ILogger logger)
		{
			_logger = logger;
			_credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(Configuration["BasicAuthentication"]!);
			_ownerRepositoryCallback = CachingManager.Cache.Memoize(TimeSpan.FromHours(8), new Func<string, Guid?>(username =>
			{
				using var scope = app.ApplicationServices.CreateScope();
				return scope.ServiceProvider.GetRequiredService<IOwnerRepository>().FindAllAsync(owner => owner.EmailAddress == username).SingleOrDefaultAsync().GetAwaiter().GetResult()?.Id;
			}));

			foreach (var sd in app.ApplicationServices.GetServiceDescriptors().Where(sd => sd.ServiceType.HasInterfaces(typeof(IPostConfigureOptions<>)) && sd.ServiceType.GenericTypeArguments[0].HasInterfaces(typeof(IExceptionDescriptorOptions))))
			{
				logger.LogInformation(sd.ServiceType.GenericTypeArguments[0].Name + " is post-configured to use same SensitivityDetails.");
			}

			logger.LogInformation("{registeredHandlers}", app.ApplicationServices.GetService<HandlerServicesDescriptor>());

			app.UseFaultDescriptorExceptionHandler();

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

			app.UseHostingEnvironment();

			app.UseCors(builder =>
			{
				builder.AllowAnyHeader();
				builder.AllowAnyMethod();
				builder.AllowAnyOrigin();
			});

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
