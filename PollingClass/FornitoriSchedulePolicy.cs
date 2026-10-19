using System;
using System.Globalization;

namespace PollingClass
{
    /// <summary>Pure scheduling policy shared by the legacy host and the future worker.</summary>
    public static class FornitoriJobDispatchPolicy
    {
        public static bool UseOneTime(bool onetime) { return onetime; }
        public static bool ParseOneTime(string value) { return Convert.ToBoolean(value); }
    }

    public sealed class ConfiguredTaskMapping
    {
        public string ProcessFamily { get; private set; }
        public Type JobType { get; private set; }
        public string OrderExportType { get; private set; }

        internal ConfiguredTaskMapping(string processFamily, Type jobType, string orderExportType)
        {
            ProcessFamily = processFamily;
            JobType = jobType;
            OrderExportType = orderExportType;
        }
    }

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

        public static ConfiguredTaskMapping MapTaskType(int taskType)
        {
            switch (taskType)
            {
                case 1: return new ConfiguredTaskMapping("Supplier", typeof(JobFornitori), null);
                case 2: return new ConfiguredTaskMapping("OrderSend", typeof(JobOrdini_SAP_Invio), "ODA");
                case 3: return new ConfiguredTaskMapping("OrderReceipt", typeof(JobOrdini_SAP_Esito), null);
                case 4: return new ConfiguredTaskMapping("Budget", typeof(JobBudget), null);
                case 5: return new ConfiguredTaskMapping("SqlExport", typeof(JobSQL01), null);
                case 6: return new ConfiguredTaskMapping("RequestSend", typeof(JobOrdini_SAP_Invio), "RDA");
                case 7: return new ConfiguredTaskMapping("Invoice", typeof(JobImportFatture), null);
                default: throw new ArgumentOutOfRangeException(nameof(taskType), "Unsupported configured task type.");
            }
        }

        public static string ProcessFamily(int taskType) { return MapTaskType(taskType).ProcessFamily; }

        public static string DailyRecurringOnetimeValue()
        {
            return "false";
        }

        public static string IntervalOnetimeValue(int intervalMinutes)
        {
            if (!IsValidInterval(intervalMinutes))
                throw new ArgumentOutOfRangeException(nameof(intervalMinutes));
            return intervalMinutes == 0 ? "true" : "false";
        }

        public static string OrderExportType(int taskType)
        {
            var mapping = MapTaskType(taskType);
            if (mapping.OrderExportType == null)
                throw new ArgumentOutOfRangeException(nameof(taskType), "Task type is not an order send.");
            return mapping.OrderExportType;
        }

        public static Type JobType(int taskType) { return MapTaskType(taskType).JobType; }

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
