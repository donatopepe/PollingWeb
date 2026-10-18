using System;
using Quartz;

namespace PollingClass
{
    /// <summary>Quartz adapter for order send and receipt jobs.</summary>
    public sealed class OrdiniQuartzGateway : IOrdiniSchedulingGateway
    {
        public bool ScheduleDaily(string institutionCode, string processId, bool sendFile,
            string tipoExport, string cronExpression, bool overrideExisting)
        {
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            IJobDetail job = sendFile
                ? JobBuilder.Create<JobOrdini_SAP_Invio>().WithIdentity("JOB_" + processId + "_" + institutionCode, "IDG").Build()
                : JobBuilder.Create<JobOrdini_SAP_Esito>().WithIdentity("JOB_" + processId + "_" + institutionCode, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            job.JobDataMap.Add("tipoExport", OrdiniJobDataPolicy.TipoExport(sendFile, tipoExport));
            job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());

            var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday };
            var normalizedCron = OrdiniSchedulerPolicy.NormalizeCron(cronExpression);
            var trigger = TriggerBuilder.Create()
                .WithIdentity("Trigger_" + processId + "_" + institutionCode, "IDG")
                .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(
                    ParseHour(normalizedCron), ParseMinute(normalizedCron), weekdays)).Build();
            if (OrdiniSchedulerPolicy.ShouldUnscheduleExistingTrigger(overrideExisting)
                && scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult()) scheduler.UnscheduleJob(trigger.Key).GetAwaiter().GetResult();
            var triggerExists = scheduler.CheckExists(trigger.Key).GetAwaiter().GetResult();
            if (!OrdiniSchedulerPolicy.CanScheduleDailyTrigger(triggerExists, overrideExisting)) return false;
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
            return true;
        }

        private static int ParseMinute(string cron)
        {
            return int.Parse(cron.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1],
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private static int ParseHour(string cron)
        {
            return int.Parse(cron.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2],
                System.Globalization.CultureInfo.InvariantCulture);
        }

        public void ScheduleInterval(int intervalMinutes, string institutionCode, string processId,
            bool sendFile, string tipoExport)
        {
            if (!OrdiniSchedulerPolicy.IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException("intervalMinutes");
            var scheduler = QuartzSchedulerFactory.Create();
            scheduler.Start().GetAwaiter().GetResult();
            var jobId = processId + "_" + institutionCode + "JOB_onetime_" + DateTime.UtcNow.Ticks;
            var triggerId = DateTime.UtcNow.Ticks.ToString();
            IJobDetail job = sendFile
                ? JobBuilder.Create<JobOrdini_SAP_Invio>().WithIdentity(jobId, "IDG").Build()
                : JobBuilder.Create<JobOrdini_SAP_Esito>().WithIdentity(jobId, "IDG").Build();
            job.JobDataMap.Add("codist", institutionCode);
            job.JobDataMap.Add("idprocesso", processId);
            job.JobDataMap.Add("tipoExport", OrdiniJobDataPolicy.TipoExport(sendFile, tipoExport));
            ITrigger trigger;
            if (intervalMinutes == 0)
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(triggerId, "IDGWEB")
                    .StartNow().WithPriority(1).Build();
            }
            else
            {
                job.JobDataMap.Add("onetime", FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes));
                trigger = TriggerBuilder.Create().WithIdentity(triggerId, "IDGWEB")
                    .StartAt(DateTimeOffset.Now.AddMinutes(intervalMinutes)).WithPriority(1).Build();
            }
            scheduler.ScheduleJob(job, trigger).GetAwaiter().GetResult();
        }
    }
}
