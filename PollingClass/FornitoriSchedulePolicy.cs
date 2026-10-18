using System;
using System.Globalization;

namespace PollingClass
{
    /// <summary>Pure scheduling policy shared by the legacy host and the future worker.</summary>
    public static class FornitoriSchedulePolicy
    {
        /// <summary>Formats a validated clock time into a Quartz weekday expression.</summary>
        public static string WeekdayCron(string timeAndHour, string weekdays)
        {
            if (string.IsNullOrWhiteSpace(timeAndHour)) throw new ArgumentException("Schedule time is required.", nameof(timeAndHour));
            if (string.IsNullOrWhiteSpace(weekdays)) throw new ArgumentException("Weekdays are required.", nameof(weekdays));
            var parts = timeAndHour.Split(new[] { ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            int first, second;
            if (parts.Length != 2 || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out first)
                || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out second)
                || !IsValidClockTime(second, first))
                throw new ArgumentException("Schedule clock time must be minute hour in invariant numeric form.", nameof(timeAndHour));
            return string.Format(CultureInfo.InvariantCulture, "0 {0} {1} ? * {2}", first, second, weekdays.Trim());
        }

        public static bool IsValidClockTime(int hour, int minute)
        {
            return hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59;
        }

        public static bool IsValidInterval(int intervalMinutes)
        {
            return intervalMinutes >= 0;
        }

        public static string ProcessFamily(int taskType)
        {
            switch (taskType)
            {
                case 1: return "Supplier";
                case 2: return "OrderSend";
                case 3: return "OrderReceipt";
                case 4: return "Budget";
                case 5: return "SqlExport";
                case 6: return "RequestSend";
                case 7: return "Invoice";
                default: throw new ArgumentOutOfRangeException(nameof(taskType), "Unsupported configured task type.");
            }
        }

        public static string OrderExportType(int taskType)
        {
            switch (taskType)
            {
                case 2: return "ODA";
                case 6: return "RDA";
                default: throw new ArgumentOutOfRangeException(nameof(taskType), "Task type is not an order send.");
            }
        }

        public static Type JobType(int taskType)
        {
            switch (taskType)
            {
                case 1: return typeof(JobFornitori);
                case 2:
                case 6: return typeof(JobOrdini_SAP_Invio);
                case 3: return typeof(JobOrdini_SAP_Esito);
                case 4: return typeof(JobBudget);
                case 5: return typeof(JobSQL01);
                case 7: return typeof(JobImportFatture);
                default: throw new ArgumentOutOfRangeException(nameof(taskType), "Unsupported configured task type.");
            }
        }

        public static string DailyIdentity(string processId, string institutionCode)
        {
            return "JOB_" + processId + "_" + institutionCode;
        }

        public static string DailyTriggerIdentity(string processId, string institutionCode)
        {
            return "Trigger_" + processId + "_" + institutionCode;
        }

        public static string WeekdayCronFromLegacy(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                throw new ArgumentException("Cron expression is required.", "cronExpression");
            var fields = cronExpression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length != 6 || fields[0] != "0" || fields[3] != "?" || fields[4] != "*")
                throw new ArgumentException("Unexpected legacy cron expression.", "cronExpression");
            if (!string.Equals(fields[5], "MON-FRI", StringComparison.Ordinal))
                throw new ArgumentException("The legacy adapter supports weekdays only.", "cronExpression");
            return cronExpression;
        }
    }

    /// <summary>Provider boundary for the two existing supplier scheduling operations.</summary>
    public interface IFornitoriSchedulingGateway
    {
        bool ScheduleDaily(string institutionCode, string processId, string cronExpression, bool overrideExisting);
        void ScheduleInterval(int intervalMinutes, string institutionCode, string processId);
    }
}
