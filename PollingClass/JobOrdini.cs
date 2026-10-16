using System;
using Database;
using Quartz;

namespace PollingClass
{
    [PersistJobDataAfterExecution]
    public class JobOrdini_SAP_Esito : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                Ordini_Sap ordini = new Ordini_Sap();
                ordini.Leggi_Esito_Sap(
                        context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"),
                        onetime);

            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }


        }
    }

    [PersistJobDataAfterExecution]
    public class JobOrdini_SAP_Invio : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                Ordini_Sap ordini = new Ordini_Sap();
                ordini.Invia_file_Sap(
                        context.JobDetail.JobDataMap.GetString("codist"),
                        context.JobDetail.JobDataMap.GetString("idprocesso"),
                        onetime,
                        context.JobDetail.JobDataMap.GetString("tipoExport"));

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


        }
    }

}
