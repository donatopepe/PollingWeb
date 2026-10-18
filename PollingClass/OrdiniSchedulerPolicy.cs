using System;

namespace PollingClass
{
    /// <summary>Provider boundary for the two order scheduler variants.</summary>
    public interface IOrdiniSchedulingGateway
    {
        bool ScheduleDaily(string institutionCode, string processId, bool sendFile, string tipoExport,
            string cronExpression, bool overrideExisting);
        void ScheduleInterval(int intervalMinutes, string institutionCode, string processId,
            bool sendFile, string tipoExport);
    }

    public sealed class OrdiniJobArguments
    {
        public bool SendFile { get; set; }
        public bool OneTime { get; set; }
        public string TipoExport { get; set; }
    }

    public static class OrdiniJobDataPolicy
    {
        public static System.Collections.Generic.IDictionary<string, string> JobData(
            string institutionCode, string processId, bool sendFile, bool oneTime, string tipoExport,
            bool includeTipoExport = true)
        {
            var arguments = Arguments(sendFile, oneTime, tipoExport);
            var data = new System.Collections.Generic.Dictionary<string, string>
            {
                ["codist"] = institutionCode,
                ["idprocesso"] = processId,
                ["onetime"] = arguments.OneTime.ToString().ToLowerInvariant()
            };
            if (includeTipoExport) data["tipoExport"] = arguments.TipoExport;
            return data;
        }

        public static System.Collections.Generic.IDictionary<string, string> IntervalJobData(
            string institutionCode, string processId, bool sendFile, int intervalMinutes, string tipoExport)
        {
            var arguments = IntervalArguments(sendFile, intervalMinutes, tipoExport);
            return JobData(institutionCode, processId, sendFile, arguments.OneTime, arguments.TipoExport);
        }
        public static string TipoExport(bool sendFile, string value)
        {
            return sendFile ? (value ?? string.Empty) : string.Empty;
        }

        public static OrdiniJobArguments IntervalArguments(bool sendFile, int intervalMinutes, string tipoExport)
        {
            return Arguments(sendFile, Convert.ToBoolean(FornitoriSchedulePolicy.IntervalOnetimeValue(intervalMinutes)), tipoExport);
        }

        public static OrdiniJobArguments Arguments(bool sendFile, bool oneTime, string tipoExport)
        {
            return new OrdiniJobArguments
            {
                SendFile = sendFile,
                OneTime = oneTime,
                TipoExport = TipoExport(sendFile, tipoExport)
            };
        }
    }

    /// <summary>Pure validation and routing policy for order scheduling.</summary>
    public static class OrdiniSchedulerPolicy
    {
        public static string NormalizeCron(string expression)
        {
            return FornitoriSchedulePolicy.WeekdayCronFromLegacy(expression);
        }

        public static bool IsValidInterval(int minutes)
        {
            return FornitoriSchedulePolicy.IsValidInterval(minutes);
        }

        public static bool IsSendJob(bool sendFile) { return sendFile; }
        public static bool IsReceiptJob(bool sendFile) { return !sendFile; }
        public static bool ShouldUnscheduleExistingTrigger(bool overrideExisting) { return overrideExisting; }
        public static bool CanScheduleDailyTrigger(bool triggerExists, bool overrideExisting)
        {
            return !triggerExists || overrideExisting;
        }
    }
}
