using System;
using System.Linq;
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

        [Fact]
        public void Daily_sql01_job_data_projection_has_legacy_three_keys()
        {
            var data = OrdiniJobDataPolicy.JobData("inst", "proc", false, false,
                string.Empty, includeTipoExport: false);
            Assert.Equal(new[] { "codist", "idprocesso", "onetime" }, data.Keys.OrderBy(key => key));
            Assert.Equal("inst", data["codist"]);
            Assert.Equal("proc", data["idprocesso"]);
            Assert.Equal("false", data["onetime"]);
            Assert.DoesNotContain("tipoExport", data.Keys);
        }

        [Fact]
        public void Daily_sql01_job_explicitly_selects_recurring_execution()
        {
            Assert.Equal("false", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
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
        [InlineData(0, "true")]
        [InlineData(30, "false")]
        public void SQL01_interval_job_data_projection_preserves_three_keys(int interval, string onetime)
        {
            var data = OrdiniJobDataPolicy.IntervalJobData("inst", "proc", interval);
            Assert.Equal(new[] { "codist", "idprocesso", "onetime" }, data.Keys.OrderBy(key => key));
            Assert.Equal("inst", data["codist"]);
            Assert.Equal("proc", data["idprocesso"]);
            Assert.Equal(onetime, data["onetime"]);
            Assert.DoesNotContain("tipoExport", data.Keys);
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
