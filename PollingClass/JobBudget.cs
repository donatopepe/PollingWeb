using System;
using System.Threading.Tasks;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobBudget : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                var budget = new ImportBudget();
                budget.Leggi_File(context.JobDetail.JobDataMap.GetString("codist"),
                    context.JobDetail.JobDataMap.GetString("idprocesso"), onetime);
            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
            return Task.CompletedTask;
        }
    }
}
