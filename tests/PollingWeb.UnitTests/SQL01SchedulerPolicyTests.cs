using System;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class SQL01SchedulerPolicyTests
    {
        [Fact]
        public void Sql01_gateway_implements_shared_scheduling_boundary()
        {
            Assert.True(typeof(IFornitoriSchedulingGateway).IsAssignableFrom(typeof(SQL01QuartzGateway)));
        }

        [Theory]
        [InlineData("0 30 8 ? * MON-FRI")]
        [InlineData("0 0 0 ? * MON-FRI")]
        [InlineData("0 59 23 ? * MON-FRI")]
        public void Daily_cron_matches_legacy_contract(string cron)
        {
            Assert.Equal(cron, FornitoriSchedulePolicy.WeekdayCronFromLegacy(cron));
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(30, true)]
        [InlineData(-1, false)]
        public void Interval_policy_accepts_zero_one_time_and_positive_values(int interval, bool expected)
        {
            Assert.Equal(expected, FornitoriSchedulePolicy.IsValidInterval(interval));
        }
    }
}
