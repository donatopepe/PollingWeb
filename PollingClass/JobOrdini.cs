using System;
using System.Threading.Tasks;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobOrdini_SAP_Esito : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var arguments = OrdiniJobDataPolicy.Arguments(false,
                    FornitoriJobDispatchPolicy.ParseOneTime(context.JobDetail.JobDataMap.GetString("onetime")),
                    context.JobDetail.JobDataMap.GetString("tipoExport"));
                Ordini_Sap ordini = new Ordini_Sap();
                ordini.Leggi_Esito_Sap(
                        context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"),
                        arguments.OneTime);

            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
            return Task.CompletedTask;
        }
    }

    [PersistJobDataAfterExecution]
    public class JobOrdini_SAP_Invio : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            try
            {
                var arguments = OrdiniJobDataPolicy.Arguments(true,
                    FornitoriJobDispatchPolicy.ParseOneTime(context.JobDetail.JobDataMap.GetString("onetime")),
                    context.JobDetail.JobDataMap.GetString("tipoExport"));
                Ordini_Sap ordini = new Ordini_Sap();
                ordini.Invia_file_Sap(
                        context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"),
                        arguments.OneTime,
                        arguments.TipoExport);

                //if (onetime)
                //{
                //    ordini.Invia_file_Sap(
                //        context.JobDetail.JobDataMap.GetString("codist"),
                //        context.JobDetail.JobDataMap.GetString("idprocesso"),
                //        onetime);
                //}
                //else
                //{
                //    ordini.Invia_file_Sap(
                //        context.JobDetail.JobDataMap.GetString("codist"),
                //        context.JobDetail.JobDataMap.GetString("idprocesso"),on);
                //}

            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
            return Task.CompletedTask;
        }
    }

}
