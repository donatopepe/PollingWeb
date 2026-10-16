using System;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class FornitoriSchedulePolicyTests
    {
        [Theory]
        [InlineData("0 8", "MON-FRI", "0 0 8 ? * MON-FRI")]
        [InlineData("35 9", "MON-FRI   ", "0 35 9 ? * MON-FRI")]
        [InlineData("5 0", "MON", "0 5 0 ? * MON")]
        public void WeekdayCron_preserves_legacy_expression(string timeAndHour, string days, string expected)
        {
            Assert.Equal(expected, FornitoriSchedulePolicy.WeekdayCron(timeAndHour, days));
        }

        [Theory]
        [InlineData(null, "MON-FRI")]
        [InlineData("", "MON-FRI")]
        [InlineData("8 0", null)]
        [InlineData("8 0", "  ")]
        public void WeekdayCron_rejects_missing_components(string timeAndHour, string days)
        {
            Assert.Throws<ArgumentException>(() => FornitoriSchedulePolicy.WeekdayCron(timeAndHour, days));
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(23, 59, true)]
        [InlineData(-1, 0, false)]
        [InlineData(24, 0, false)]
        [InlineData(8, 60, false)]
        public void Clock_policy_checks_range(int hour, int minute, bool expected)
        {
            Assert.Equal(expected, FornitoriSchedulePolicy.IsValidClockTime(hour, minute));
        }
        [Theory]
        [InlineData(0, true)]
        [InlineData(15, true)]
        [InlineData(-1, false)]
        public void Interval_policy_rejects_negative_polling(int minutes, bool expected)
        {
            Assert.Equal(expected, FornitoriSchedulePolicy.IsValidInterval(minutes));
        }
    }
}
