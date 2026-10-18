using System;
using Quartz;

namespace PollingClass
{
    /// <summary>Quartz adapter for the SQL01 export polling schedule.</summary>
    public sealed class SQL01QuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var job = JobBuilder.Create<JobSQL01>()
                .WithIdentity(FornitoriSchedulePolicy.DailyIdentity(processId, institutionCode), "IDG").Build();
            var jobData = OrdiniJobDataPolicy.JobData(institutionCode, processId,
                false, false, string.Empty, includeTipoExport: false);
            foreach (var entry in jobData) job.JobDataMap.Add(entry.Key, entry.Value);
            var normalized = FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression);
            var trigger = TriggerBuilder.Create()
                .WithIdentity(FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithCronSchedule(normalized).Build();
            var triggerExists = scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult();
            if (triggerExists && OrdiniSchedulerPolicy.ShouldUnscheduleExistingTrigger(overrideExisting))
                scheduler.UnscheduleJob(trigger.Key).GetAwaiter().GetResult();
            if (!OrdiniSchedulerPolicy.CanScheduleDailyTrigger(
                scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult(), overrideExisting)) return false;
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
            return true;
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId)
        {
            if (!FornitoriSchedulePolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var identity = processId + "_" + institutionCode + "JOB_onetime_" + DateTime.UtcNow.Ticks;
            var job = JobBuilder.Create<JobSQL01>().WithIdentity(identity, "IDG").Build();
            var jobData = OrdiniJobDataPolicy.IntervalJobData(institutionCode, processId, intervalMinutes);
            foreach (var entry in jobData) job.JobDataMap.Add(entry.Key, entry.Value);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG").StartNow().Build();
            }
            else
            {
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG")
                    .StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }
    }
}
