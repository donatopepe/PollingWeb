using System;
using Quartz;
using Quartz.Impl;

namespace PollingClass
{
    /// <summary>Quartz 2.x adapter for invoice-import schedules, legacy net48 host.</summary>
    public sealed class ImportFattureQuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.Start();
            var job = JobBuilder.Create<JobImportFatture>()
                .WithIdentity("JOB_" + processId + "_" + institutionCode, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            var trigger = TriggerBuilder.Create()
                .WithIdentity("Trigger_" + processId + "_" + institutionCode, "IDG")
                .WithCronSchedule(FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression)).Build();
            if (scheduler.CheckExists(trigger.Key) && overrideExisting)
                scheduler.UnscheduleJob(trigger.Key);
            if (scheduler.CheckExists(trigger.Key)) return false;
            scheduler.ScheduleJob(job, trigger);
            return true;
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId)
        {
            if (!FornitoriSchedulePolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.Start();
            var ticks = DateTime.Now.Ticks;
            var job = JobBuilder.Create<JobImportFatture>().WithIdentity(
                processId + "_" + institutionCode + "JOB_onetime_" + ticks, "IDG").Build();
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
