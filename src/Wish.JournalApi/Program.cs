using System;
using System.Threading.Tasks;
using Azure.Identity;
using Codebelt.Bootstrapper.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Wish.JournalApi
{
	public class Program : WebProgram<Startup>
	{
		public static Task Main(string[] args)
		{
			return CreateHostBuilder(args)
			  .ConfigureAppConfiguration(builder =>
				{
					var config = builder.Build();
					builder.AddAzureKeyVault(new Uri($"https://{config["Azure:KeyVault:Name"]}.vault.azure.net/"), new DefaultAzureCredential());
				})
				.Build()
				.RunAsync();
		}
	}
}
