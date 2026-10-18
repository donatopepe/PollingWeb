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
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
            var normalized = FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression);
            var trigger = TriggerBuilder.Create()
                .WithIdentity(FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithCronSchedule(normalized).Build();
            if (overrideExisting && scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult()) scheduler.UnscheduleJob(trigger.Key).GetAwaiter().GetResult();
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
            var identity = processId + "_" + institutionCode + "JOB_onetime_" + DateTime.UtcNow.Ticks;
            var job = JobBuilder.Create<JobSQL01>().WithIdentity(identity, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", "true");
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG").StartNow().Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", "false");
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG")
                    .StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }
    }
}
