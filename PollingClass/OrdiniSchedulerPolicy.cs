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

    public static class OrdiniJobDataPolicy
    {
        public static string TipoExport(bool sendFile, string value)
        {
            return sendFile ? (value ?? string.Empty) : string.Empty;
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
    }
}
