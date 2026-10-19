using System;
using System.Linq;
using PollingClass;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class ImportBudgetScheduleAdapterTests
    {
        [Fact]
        public void Budget_gateway_implements_scheduling_boundary()
        {
            Assert.True(typeof(IFornitoriSchedulingGateway).IsAssignableFrom(typeof(ImportBudgetQuartzGateway)));
        }

        [Fact]
        public void Daily_budget_job_data_projection_has_legacy_three_keys()
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
        public void Daily_budget_jobs_explicitly_select_recurring_execution()
        {
            Assert.Equal("false", FornitoriSchedulePolicy.DailyRecurringOnetimeValue());
        }

        [Theory]
        [InlineData("0 30 8 ? * MON-FRI")]
        [InlineData("0 5 0 ? * MON-FRI")]
        [InlineData("0 59 23 ? * MON-FRI")]
        public void Budget_gateway_accepts_existing_weekday_cron_contract(string cron)
        {
            Assert.Equal(cron, FornitoriSchedulePolicy.WeekdayCronFromLegacy(cron));
        }


        [Theory]
        [InlineData("0 30 8 ? * TUE")]
        [InlineData("30 8 * * MON-FRI")]
        [InlineData("")]
        public void Budget_gateway_rejects_nonlegacy_cron(string cron)
        {
            Assert.Throws<ArgumentException>(() => FornitoriSchedulePolicy.WeekdayCronFromLegacy(cron));
        }
    }
}
