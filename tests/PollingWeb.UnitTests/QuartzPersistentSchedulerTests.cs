using System;
using System.Threading.Tasks;
using Quartz;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class QuartzPersistentSchedulerTests
    {
        [Fact]
        public void Scheduler_requires_explicit_database_connection()
        {
            Assert.Throws<InvalidOperationException>(() => QuartzSchedulerFactory.Create(string.Empty));
        }

        [Fact(Skip = "This persistent-store scenario runs in PollingWeb.IntegrationTests against the SQL Server fixture.")]
        public async Task Ado_store_registers_trigger_surviving_scheduler_restart()
        {
            var connectionString = Environment.GetEnvironmentVariable("POLLING_QUARTZ_CONNECTION");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Set POLLING_QUARTZ_CONNECTION to run the Quartz SQL integration test.");

            var schedulerName = "PollingWebTest" + Guid.NewGuid().ToString("N");
            var scheduler = QuartzSchedulerFactory.Create(connectionString, schedulerName);
            await scheduler.Start();
            var job = JobBuilder.Create<QuartzPersistenceProbeJob>()
                .WithIdentity("probe-job", "probe-group")
                .StoreDurably()
                .Build();
            await scheduler.AddJob(job, true);
            var trigger = TriggerBuilder.Create()
                .WithIdentity("probe-trigger", "probe-group")
                .ForJob(job.Key)
                .StartAt(DateTimeOffset.UtcNow.AddHours(1))
                .Build();
            await scheduler.ScheduleJob(trigger);
            Assert.True(await scheduler.CheckExists(trigger.Key));
            await scheduler.Shutdown(true);

            var restarted = QuartzSchedulerFactory.Create(connectionString, schedulerName);
            await restarted.Start();
            Assert.True(await restarted.CheckExists(trigger.Key));
            await restarted.DeleteJob(job.Key);
            await restarted.Shutdown(true);
        }

        public sealed class QuartzPersistenceProbeJob : IJob
        {
            public Task Execute(IJobExecutionContext context) => Task.CompletedTask;
        }
    }
}
