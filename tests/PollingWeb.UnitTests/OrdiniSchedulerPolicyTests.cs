using System;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class OrdiniSchedulerPolicyTests
    {
        [Theory]
        [InlineData(true, "ODA", "ODA")]
        [InlineData(true, "RDA", "RDA")]
        [InlineData(true, null, "")]
        [InlineData(false, "ODA", "")]
        [InlineData(false, null, "")]
        public void Job_data_export_type_is_present_only_for_send_jobs(bool sendFile, string input, string expected)
        {
            Assert.Equal(expected, OrdiniJobDataPolicy.TipoExport(sendFile, input));
        }

        [Fact]
        public void Gateway_exposes_two_job_types_without_changing_send_receipt_routing()
        {
            Assert.True(typeof(OrdiniQuartzGateway).IsClass);
            Assert.True(typeof(IOrdiniSchedulingGateway).IsAssignableFrom(typeof(OrdiniQuartzGateway)));
            Assert.True(OrdiniSchedulerPolicy.IsSendJob(true));
            Assert.False(OrdiniSchedulerPolicy.IsReceiptJob(true));
            Assert.True(OrdiniSchedulerPolicy.IsReceiptJob(false));
            Assert.False(OrdiniSchedulerPolicy.IsSendJob(false));
        }

        [Fact]
        public void Daily_order_jobs_explicitly_select_recurring_execution()
        {
            Assert.Equal("false", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Override_policy_controls_existing_daily_trigger_replacement(bool overrideExisting, bool shouldUnschedule)
        {
            Assert.Equal(shouldUnschedule, OrdiniSchedulerPolicy.ShouldUnscheduleExistingTrigger(overrideExisting));
        }

        [Theory]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void Daily_trigger_duplicate_policy_preserves_override_contract(bool triggerExists, bool overrideExisting, bool expected)
        {
            Assert.Equal(expected, OrdiniSchedulerPolicy.CanScheduleDailyTrigger(triggerExists, overrideExisting));
        }

        [Theory]
        [InlineData("0 30 8 ? * MON-FRI")]
        [InlineData("0 0 0 ? * MON-FRI")]
        [InlineData("0 59 23 ? * MON-FRI")]
        public void Daily_policy_preserves_legacy_cron(string cron)
        {
            Assert.Equal(cron, OrdiniSchedulerPolicy.NormalizeCron(cron));
        }

        [Theory]
        [InlineData("0 30 8 ? * SUN")]
        [InlineData("30 8 ? * MON-FRI")]
        [InlineData("")]
        public void Daily_policy_rejects_invalid_cron(string cron)
        {
            Assert.Throws<ArgumentException>(() => OrdiniSchedulerPolicy.NormalizeCron(cron));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(10, true)]
        [InlineData(-1, false)]
        public void Interval_policy_keeps_zero_as_onetime(int minutes, bool expected)
        {
            Assert.Equal(expected, OrdiniSchedulerPolicy.IsValidInterval(minutes));
        }
    }
}
