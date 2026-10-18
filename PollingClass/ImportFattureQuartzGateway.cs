using System;
using Quartz;

namespace PollingClass
{
    /// <summary>Quartz adapter for invoice-import schedules, legacy net48 host.</summary>
    public sealed class ImportFattureQuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var job = JobBuilder.Create<JobImportFatture>()
                .WithIdentity("JOB_" + processId + "_" + institutionCode, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
            var trigger = TriggerBuilder.Create()
                .WithIdentity("Trigger_" + processId + "_" + institutionCode, "IDG")
                .WithCronSchedule(FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression)).Build();
            if (scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult() && overrideExisting)
                scheduler.UnscheduleJob(trigger.Key).GetAwaiter().GetResult();
            if (scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult()) return false;
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
            return true;
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId)
        {
            if (!FornitoriSchedulePolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var ticks = DateTime.UtcNow.Ticks;
            var job = JobBuilder.Create<JobImportFatture>().WithIdentity(
                processId + "_" + institutionCode + "JOB_onetime_" + ticks, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "Trigger_onetime_" + DateTime.UtcNow.Ticks, "IDG").StartNow().Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(processId + "_" + institutionCode +
                    "_" + DateTime.UtcNow.Ticks, "IDG").StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }

    }
}
