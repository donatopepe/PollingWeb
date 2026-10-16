using System;
using Database.Configuration;
using Quartz;
using Quartz.Impl;

namespace PollingClass
{
    /// <summary>Creates the application's persistent Quartz 3.x scheduler on the Quartz SQL database.</summary>
    public static class QuartzSchedulerFactory
    {
        public static IAppConfig Configuration { get; set; } = new AppConfig();
        public static void ValidateQuartzConnectionString(string value)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(value);
            if (string.IsNullOrWhiteSpace(builder.DataSource) || string.IsNullOrWhiteSpace(builder.InitialCatalog))
                throw new InvalidOperationException("Quartz SQL connection must specify server and database.");
        }

        public static IScheduler Create(string connectionString = null, string instanceName = "PollingWebScheduler")
        {
            if (string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("Scheduler instance name is required.", nameof(instanceName));
            var resolved = connectionString;
            if (!string.IsNullOrWhiteSpace(resolved))
                ValidateQuartzConnectionString(resolved);
            if (string.IsNullOrWhiteSpace(resolved))
            {
                resolved = Environment.GetEnvironmentVariable("POLLING_QUARTZ_CONNECTION");
                if (string.IsNullOrWhiteSpace(resolved))
                    resolved = Configuration.GetConnectionString("QuartzConnectionString");
                else
                    ValidateQuartzConnectionString(resolved);
            }
            if (string.IsNullOrWhiteSpace(resolved))
                throw new InvalidOperationException("Quartz connection is not configured. Set POLLING_QUARTZ_CONNECTION or QuartzConnectionString.");

            var properties = new System.Collections.Specialized.NameValueCollection
            {
                ["quartz.scheduler.instanceName"] = instanceName,
                ["quartz.scheduler.instanceId"] = "AUTO",
                ["quartz.threadPool.type"] = "Quartz.Simpl.DefaultThreadPool, Quartz",
                ["quartz.threadPool.threadCount"] = "5",
                ["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
                ["quartz.jobStore.dataSource"] = "default",
                ["quartz.jobStore.tablePrefix"] = "QRTZ_",
                ["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.SqlServerDelegate, Quartz",
                ["quartz.jobStore.useProperties"] = "true",
                ["quartz.serializer.type"] = "json",
                ["quartz.jobStore.clustered"] = "false",
                ["quartz.jobStore.misfireThreshold"] = "60000",
                ["quartz.dataSource.default.provider"] = "MicrosoftDataSqlClient",
                ["quartz.dataSource.default.connectionString"] = resolved,
            };
            return new StdSchedulerFactory(properties).GetScheduler().GetAwaiter().GetResult();
        }
    }


}
