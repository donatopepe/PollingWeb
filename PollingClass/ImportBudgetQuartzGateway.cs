using System;
using Quartz;

namespace PollingClass
{
    /// <summary>Quartz adapter for budget-import schedules, legacy net48 host.</summary>
    public sealed class ImportBudgetQuartzGateway : IFornitoriSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting)
        {
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var job = JobBuilder.Create<JobBudget>()
                .WithIdentity(FornitoriSchedulePolicy.DailyIdentity(processId, institutionCode), "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
            var legacyCron = FornitoriSchedulePolicy.WeekdayCronFromLegacy(cronExpression);
            var trigger = TriggerBuilder.Create()
                .WithIdentity(FornitoriSchedulePolicy.DailyTriggerIdentity(processId, institutionCode), "IDG")
                .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(
                    ParseHour(legacyCron), ParseMinute(legacyCron), Weekdays())).Build();
            if (OrdiniSchedulerPolicy.ShouldUnscheduleExistingTrigger(overrideExisting))
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
            var identity = processId + "_" + institutionCode + "JOB_onetime_" + DateTime.UtcNow.Ticks;
            var job = JobBuilder.Create<JobBudget>().WithIdentity(identity, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG").StartNow().Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(identity, "IDG")
                    .StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }

        private static DayOfWeek[] Weekdays()
        {
            return new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday };
        }

        private static int ParseMinute(string cron)
        {
            var fields = cron.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return int.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
        }

        private static int ParseHour(string cron)
        {
            var fields = cron.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return int.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
