using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using PollingClass;

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
        private IScheduler scheduler;

        public Worker(ILogger<Worker> logger)
        {
            this.logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken).ConfigureAwait(false);
            scheduler = QuartzSchedulerFactory.Create();
            await scheduler.Start(cancellationToken).ConfigureAwait(false);
            logger.LogInformation("PollingWorker started persistent Quartz scheduler {SchedulerName}.", scheduler.SchedulerName);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("PollingWorker is ready; legacy source scanning/job registration is pending X2.3.");
            return Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (scheduler != null && !scheduler.IsShutdown)
                await scheduler.Shutdown(true, cancellationToken).ConfigureAwait(false);
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
