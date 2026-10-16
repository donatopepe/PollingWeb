using System;
using System.Threading.Tasks;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobFornitori : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                var fornitore = new LeggiFornitore();
                if (onetime)
                    fornitore.LetturaFor_ONETIME(context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"));
                else
                    fornitore.LetturaFor(context.JobDetail.JobDataMap.GetString("codist"),
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
