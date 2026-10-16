using System;
using Quartz;
using Quartz.Impl;

namespace PollingClass
{
    /// <summary>Quartz 2.x adapter for the legacy net48 supplier scheduler.</summary>
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
            IScheduler scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.Start();
            var job = JobBuilder.Create<JobFornitori>().WithIdentity(
                FornitoriSchedulePolicy.DailyIdentity(processId, institutionCode), "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            var trigger = TriggerBuilder.Create().WithIdentity(
                FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithCronSchedule(FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression)).Build();
            if (overrideExisting) scheduler.UnscheduleJob(trigger.Key);
            if (scheduler.CheckExists(trigger.Key)) return false;
            scheduler.ScheduleJob(job, trigger);
            return true;
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId)
        {
            if (!FornitoriSchedulePolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            IScheduler scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.Start();
            var job = JobBuilder.Create<JobFornitori>().WithIdentity(
                processId + "_" + institutionCode + "JOB_onetime_" + DateTime.Now.Ticks, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", "true");
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "Trigger_onetime_" + DateTime.Now.Ticks, "IDG").StartNow().Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", "false");
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "_" + DateTime.Now.Ticks, "IDG").StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
