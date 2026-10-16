using System;
using Quartz;
using Quartz.Impl;

namespace PollingClass
{
    /// <summary>Quartz 2.x adapter for the SQL01 export polling schedule.</summary>
    public sealed class SQL01QuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            var scheduler = new StdSchedulerFactory().GetScheduler();
            scheduler.Start();
            var job = JobBuilder.Create<JobSQL01>()
                .WithIdentity(FornitoriSchedulePolicy.DailyIdentity(processId, institutionCode), "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            var normalized = FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression);
            var trigger = TriggerBuilder.Create()
                .WithIdentity(FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithCronSchedule(normalized).Build();
            if (overrideExisting && scheduler.CheckExists(trigger.Key)) scheduler.UnscheduleJob(trigger.Key);
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
            var identity = processId + "_" + institutionCode + "JOB_onetime_" + DateTime.Now.Ticks;
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
            scheduler.ScheduleJob(job, trigger);
        }
    }
}
