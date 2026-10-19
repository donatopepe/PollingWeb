using System;
using System.Linq;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class OrdiniSchedulerPolicyTests
    {
        [Theory]
        [InlineData(0, "true")]
        [InlineData(15, "false")]
        public void SQL01_interval_projection_preserves_three_key_map(int interval, string onetime)
        {
            var data = OrdiniJobDataPolicy.IntervalJobData("inst", "proc", interval);
            Assert.Equal(new[] { "codist", "idprocesso", "onetime" }, data.Keys.OrderBy(key => key));
            Assert.Equal("inst", data["codist"]);
            Assert.Equal("proc", data["idprocesso"]);
            Assert.Equal(onetime, data["onetime"]);
        }

        [Theory]
        [InlineData(true, "ODA", "ODA")]
        [InlineData(false, "ODA", "")]
        public void Daily_order_job_data_projection_preserves_legacy_keys_and_values(bool sendFile,
            string selector, string expectedSelector)
        {
            var data = OrdiniJobDataPolicy.JobData("inst", "proc", sendFile, false, selector);
            Assert.Equal(new[] { "codist", "idprocesso", "onetime", "tipoExport" }, data.Keys.OrderBy(key => key));
            Assert.Equal("inst", data["codist"]);
            Assert.Equal("proc", data["idprocesso"]);
            Assert.Equal("false", data["onetime"]);
            Assert.Equal(expectedSelector, data["tipoExport"]);
        }

        [Fact]
        public void Interval_job_data_projection_preserves_receipt_keys_and_flags()
        {
            var data = OrdiniJobDataPolicy.IntervalJobData("inst", "proc", false, 0, "ODA");
            Assert.Equal("inst", data["codist"]);
            Assert.Equal("proc", data["idprocesso"]);
            Assert.Equal("true", data["onetime"]);
            Assert.Equal(string.Empty, data["tipoExport"]);
        }

        [Theory]
        [InlineData(true, 0, "ODA", true, "ODA")]
        [InlineData(true, 5, "ODA", false, "ODA")]
        [InlineData(false, 0, "RDA", true, "")]
        [InlineData(false, 30, null, false, "")]
        public void Interval_job_arguments_preserve_job_data_contract(bool sendFile, int interval,
            string selector, bool expectedOneTime, string expectedExport)
        {
            var arguments = OrdiniJobDataPolicy.IntervalArguments(sendFile, interval, selector);
            Assert.Equal(sendFile, arguments.SendFile);
            Assert.Equal(expectedOneTime, arguments.OneTime);
            Assert.Equal(expectedExport, arguments.TipoExport);
        }

        [Theory]
        [InlineData(true, true, "ODA", true, "ODA")]
        [InlineData(true, false, "RDA", false, "RDA")]
        [InlineData(false, true, "ODA", true, "")]
        [InlineData(false, false, null, false, "")]
        public void Job_arguments_preserve_send_receipt_one_time_and_export_contract(bool sendFile, bool oneTime,
            string selector, bool expectedOneTime, string expectedExport)
        {
            var arguments = OrdiniJobDataPolicy.Arguments(sendFile, oneTime, selector);
            Assert.Equal(sendFile, arguments.SendFile);
            Assert.Equal(expectedOneTime, arguments.OneTime);
            Assert.Equal(expectedExport, arguments.TipoExport);
        }

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
