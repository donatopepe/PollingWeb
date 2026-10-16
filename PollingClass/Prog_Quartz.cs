using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Database.Base;
using System.Collections;
using Database;
using Database.TableInterface;

/// <summary>
/// Scheduling fornitori, indipendente dal provider.
/// </summary>


namespace PollingClass
{
    public class Fornitori_Scheduler
    {
        public string messaggio_errore = "";
        private readonly IFornitoriSchedulingGateway schedulingGateway;

        public Fornitori_Scheduler() : this(FornitoriSchedulingGatewayFactory.Create()) { }

        public Fornitori_Scheduler(IFornitoriSchedulingGateway schedulingGateway)
        {
            this.schedulingGateway = schedulingGateway ?? throw new ArgumentNullException(nameof(schedulingGateway));
        }

        public bool FirstStart(string codist, string idprocesso)
        {

            return this.FirstStart(codist, idprocesso, false);
        }

        public bool FirstStart(string codist, string idprocesso, bool overrideTask)
        {

            //Log_Errori.ScriviLOGPROCEDURE("Fornitori_Scheduler.FirstStart ");

            bool esito = true;
            DatabaseInfo dbInfo = new DatabaseInfo();
            dbInfo.IniziaSessione();
            dtsTabelle dts = new dtsTabelle();

            ArrayList array = new ArrayList();
            FiltroCampi fc;

            fc = new FiltroCampi();
            fc.Campo = "Istituto";
            fc.Valore = codist;
            fc.Tipo = "N";
            array.Add(fc);

            fc = new FiltroCampi();
            fc.Campo = "IdProcesso";
            fc.Valore = idprocesso;
            array.Add(fc);

            TableAdapterTaskService tbiTaskService = new TableAdapterTaskService(dbInfo, dts);
            tbiTaskService.Load(array);
            try
            {
                int min_start, ora_start;
                string orario_start = "0 8"; //8 per default
                string withCronSchedule = FornitoriSchedulePolicy.WeekdayCron("30 8", "MON-FRI");
                if (dts.TaskService.Rows.Count == 1)
                {
                    min_start = Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Minute;
                    ora_start = Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Hour;

                    orario_start =
                      Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Minute.ToString() + " " +
                      Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Hour.ToString();

                    // Il gateway conserva la ricorrenza feriale legacy.
                    withCronSchedule = FornitoriSchedulePolicy.WeekdayCron(orario_start, "MON-FRI");
                }
                else
                {
                    dbInfo.TerminaSessione();
                    esito = false;
                    //codice istituto+idprocesso non trovato
                    throw new PollingException(PollingExceptionType.Task_non_trovato, "");

                }

                dbInfo.TerminaSessione();

                if (!this.schedulingGateway.ScheduleDaily(codist, idprocesso, withCronSchedule, overrideTask))
                {
                    esito = false;
                    throw new PollingException(PollingExceptionType.Job_Quartz_esistente, "");
                }
            }
            catch (PollingException ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "FirstStart exception " + ex.Message);

                messaggio_errore = PollingExceptionHandler.Esegui(ex);
            }

            return esito;
        }

        public bool Polling_Interval(int minuti, string codist, string idprocesso)
        {
            bool esito = true;
            try
            {
                //Log_Errori.ScriviLOGPROCEDURE("Polling_Interval ");


                this.schedulingGateway.ScheduleInterval(minuti, codist, idprocesso);
            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "Polling_Interval exception");

                esito = false;
                messaggio_errore = PollingExceptionHandler.Esegui(ex);
            }

            return esito;
        }
    }


}




