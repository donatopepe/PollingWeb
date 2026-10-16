using System;
using System.Threading.Tasks;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobSQL01 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                ExportSQL01 export = new ExportSQL01();
                export.Leggi_File(
                        context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"),
                        onetime);

                Log_Errori.ScriviLOGPROCEDURE("", "", "Execute JobSQL01");

            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE("", "", "errore Execute JobSQL01 " + ex.Message);
                PollingExceptionHandler.Esegui(ex);
            }
            return Task.CompletedTask;
        }
    }
}
