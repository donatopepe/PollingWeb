using System;
using System.Threading.Tasks;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobImportFatture : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = FornitoriJobDispatchPolicy.ParseOneTime(
                    context.JobDetail.JobDataMap.GetString("onetime"));
                var job = new ImportFatture();
                if (FornitoriJobDispatchPolicy.UseOneTime(onetime))
                    job.Lettura_ONETIME(context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"));
                else
                    job.Lettura(context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"));
            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
            return Task.CompletedTask;
        }
    }
}
