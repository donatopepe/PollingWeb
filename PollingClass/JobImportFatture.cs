using System;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobImportFatture : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                var job = new ImportFatture();
                if (onetime)
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
        }
    }
}
