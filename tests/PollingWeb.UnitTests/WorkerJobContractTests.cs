using System;
using System.Collections.Generic;
using System.Linq;
using PollingClass;
using Quartz;
using Xunit;

namespace PollingWeb.UnitTests
{
    public class WorkerJobContractTests
    {
        public static IEnumerable<object[]> JobTypes()
        {
            yield return new object[] { typeof(JobFornitori) };
            yield return new object[] { typeof(JobBudget) };
            yield return new object[] { typeof(JobImportFatture) };
            yield return new object[] { typeof(JobSQL01) };
            yield return new object[] { typeof(JobOrdini_SAP_Esito) };
            yield return new object[] { typeof(JobOrdini_SAP_Invio) };
        }

        [Theory]
        [InlineData(1, "Supplier", typeof(JobFornitori))]
        [InlineData(2, "OrderSend", typeof(JobOrdini_SAP_Invio))]
        [InlineData(3, "OrderReceipt", typeof(JobOrdini_SAP_Esito))]
        [InlineData(4, "Budget", typeof(JobBudget))]
        [InlineData(5, "SqlExport", typeof(JobSQL01))]
        [InlineData(6, "RequestSend", typeof(JobOrdini_SAP_Invio))]
        [InlineData(7, "Invoice", typeof(JobImportFatture))]
        public void Configured_task_type_maps_to_legacy_job_family(int taskType, string family, Type expectedJobType)
        {
            Assert.Equal(family, FornitoriSchedulePolicy.ProcessFamily(taskType));
            Assert.Equal(expectedJobType, FornitoriSchedulePolicy.JobType(taskType));
        }

        [Theory]
        [InlineData(2, "ODA")]
        [InlineData(6, "RDA")]
        public void Order_task_type_preserves_legacy_export_selector(int taskType, string expectedExport)
        {
            Assert.Equal(expectedExport, FornitoriSchedulePolicy.OrderExportType(taskType));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(7)]
        public void Non_order_task_type_cannot_select_order_export(int taskType)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FornitoriSchedulePolicy.OrderExportType(taskType));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(8)]
        public void Unknown_task_type_is_rejected(int taskType)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FornitoriSchedulePolicy.JobType(taskType));
            Assert.Throws<ArgumentOutOfRangeException>(() => FornitoriSchedulePolicy.ProcessFamily(taskType));
        }

        [Theory]
        [MemberData(nameof(JobTypes))]
        public void Daily_recurring_job_data_value_is_consumed_as_false_by_each_job(Type jobType)
        {
            var onetime = FornitoriSchedulePolicy.DailyRecurringOnetimeValue();
            Assert.Equal("false", onetime);
            Assert.False(Convert.ToBoolean(onetime));
            Assert.True(typeof(IJob).IsAssignableFrom(jobType));
        }

        [Theory]
        [MemberData(nameof(JobTypes))]
        public void Legacy_job_types_are_constructible_and_expose_Quartz_execute_contract(Type jobType)
        {
            Assert.NotNull(Activator.CreateInstance(jobType));
            Assert.True(typeof(IJob).IsAssignableFrom(jobType));
            var execute = jobType.GetMethod("Execute");
            Assert.NotNull(execute);
            Assert.Equal(typeof(System.Threading.Tasks.Task), execute.ReturnType);
            Assert.Equal(new[] { typeof(IJobExecutionContext) }, Array.ConvertAll(execute.GetParameters(), parameter => parameter.ParameterType));
            Assert.DoesNotContain(jobType.Assembly.GetReferencedAssemblies(), reference => reference.Name == "System.Web");
            Assert.Contains(typeof(PersistJobDataAfterExecutionAttribute), jobType.GetCustomAttributes(false).Select(attribute => attribute.GetType()));
        }
    }
}
