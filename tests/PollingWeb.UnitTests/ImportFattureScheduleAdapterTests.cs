using System;
using System.Collections.Generic;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class ImportFattureScheduleAdapterTests
    {
        [Fact]
        public void Invoice_schedule_adapter_implements_shared_provider_boundary()
        {
            Assert.True(typeof(IFornitoriSchedulingGateway).IsAssignableFrom(typeof(ImportFattureQuartzGateway)));
        }

        [Theory]
        [InlineData("0 30 8 ? * MON-FRI", 8, 30)]
        [InlineData("0 0 0 ? * MON-FRI", 0, 0)]
        [InlineData("0 59 23 ? * MON-FRI", 23, 59)]
        public void Legacy_cron_validation_accepts_weekday_schedules(string expression, int hour, int minute)
        {
            Assert.Equal(expression, FornitoriSchedulePolicy.WeekdayCronFromLegacy(expression));
            Assert.True(FornitoriSchedulePolicy.IsValidClockTime(hour, minute));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(30, true)]
        [InlineData(-1, false)]
        public void Polling_interval_validation_is_consistent(int minutes, bool expected)
        {
            Assert.Equal(expected, FornitoriSchedulePolicy.IsValidInterval(minutes));
        }

        [Fact]
        public void Daily_job_and_trigger_identities_match_legacy_contract()
        {
            Assert.Equal("JOB_INV_7", FornitoriSchedulePolicy.DailyIdentity("INV", "7"));
            Assert.Equal("Trigger_INV_7", FornitoriSchedulePolicy.DailyTriggerIdentity("INV", "7"));
        }
    }
}
