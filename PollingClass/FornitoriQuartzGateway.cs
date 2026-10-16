using System;
using Quartz;

namespace PollingClass
{
    /// <summary>Quartz adapter for the legacy net48 supplier scheduler.</summary>
    public static class FornitoriSchedulingGatewayFactory
    {
        public static IFornitoriSchedulingGateway Create()
        {
            return new FornitoriQuartzGateway();
        }
    }

    public sealed class FornitoriQuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            IScheduler scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var job = JobBuilder.Create<JobFornitori>().WithIdentity(
                FornitoriSchedulePolicy.DailyIdentity(processId, institutionCode), "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            var trigger = TriggerBuilder.Create().WithIdentity(
                FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithCronSchedule(FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression)).Build();
            if (overrideExisting) scheduler.UnscheduleJob(trigger.Key).GetAwaiter().GetResult();
            if (scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult()) return false;
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
            return true;
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId)
        {
            if (!FornitoriSchedulePolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            IScheduler scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var job = JobBuilder.Create<JobFornitori>().WithIdentity(
                processId + "_" + institutionCode + "JOB_onetime_" + DateTime.UtcNow.Ticks, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", "true");
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "Trigger_onetime_" + DateTime.UtcNow.Ticks, "IDG").StartNow().Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", "false");
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "_" + DateTime.UtcNow.Ticks, "IDG").StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }
    }
}
