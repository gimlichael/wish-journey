using System;
using System.Threading.Tasks;
using Azure.Identity;
using Codebelt.Bootstrapper.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Wish.JournalEventSvc
{
    internal class Program : WorkerProgram<Startup>
    {
        static Task Main(string[] args)
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
