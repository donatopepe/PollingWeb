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
