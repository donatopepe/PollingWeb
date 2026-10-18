using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PollingWorker
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            using (IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => services.AddHostedService<Worker>())
                .Build())
            {
                await host.RunAsync().ConfigureAwait(false);
            }
        }
    }

    internal sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("PollingWorker host started; job and source scanning integration is pending X2.3.");
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
