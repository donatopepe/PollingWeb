using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using Quartz;
using Quartz.Impl;
using Database.Base;
using System.Collections;
using Database;
using Database.TableInterface;

using Quartz.Impl.Matchers;
/// <summary>
/// Descrizione di riepilogo per Prog_Quartz
/// </summary>


namespace PollingClass
{
    public class Fornitori_Scheduler
    {
        public string messaggio_errore = "";

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
                string day_start = "MON-FRI";
                string withCronSchedule = "0 30 8 ? * MON-FRI";
                if (dts.TaskService.Rows.Count == 1)
                {
                    min_start = Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Minute;
                    ora_start = Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Hour;

                    orario_start =
                      Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Minute.ToString() + " " +
                      Convert.ToDateTime(dts.TaskService.Rows[0]["Orario_Check_File"].ToString()).Hour.ToString();

                    day_start = dts.TaskService.Rows[0]["Day_Check_File"].ToString();
                    withCronSchedule = "0 " + orario_start + " ? * " + day_start.Trim();
                }
                else
                {
                    dbInfo.TerminaSessione();
                    esito = false;
                    //codice istituto+idprocesso non trovato
                    throw new PollingException(PollingExceptionType.Task_non_trovato, "");

                }

                dbInfo.TerminaSessione();

                //Log_Errori.ScriviLOGPROCEDURE("FirstStart withCronSchedule " + withCronSchedule);

                ISchedulerFactory schedFact = new StdSchedulerFactory();
                IScheduler quartzScheduler = schedFact.GetScheduler();
                var workingDays = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

                quartzScheduler.Start();

                IJobDetail job = JobBuilder.Create<JobFornitori>().WithIdentity("JOB_" + idprocesso + "_" + codist, "IDG").Build();
                job.JobDataMap.Add("codist", codist);
                job.JobDataMap.Add("idprocesso", idprocesso);


                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity("Trigger_" + idprocesso + "_" + codist, "IDG")
                  //.WithCronSchedule(withCronSchedule) //dal lun-ven, tutti i mesi alle 8.30                                                                             
                  .WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(ora_start, min_start, workingDays))
                  //.StartAt(DateTimeOffset.Now)
                  //.WithPriority(1)
                  .Build();


                //cancello trigger
                if (overrideTask)
                    quartzScheduler.UnscheduleJob(trigger.Key);

                if (quartzScheduler.CheckExists(trigger.Key))
                {
                    esito = false;
                    throw new PollingException(PollingExceptionType.Job_Quartz_esistente, "");
                }

                quartzScheduler.ScheduleJob(job, trigger);
                //quartzScheduler.Start();


                //DEMO
                //string withCronSchedule = "0 40/1 9 ? * MON-FRI";
                //IJobDetail job = JobBuilder.Create<Job_Test2>().Build();

                //ITrigger trigger = TriggerBuilder.Create()
                //        .WithIdentity("Job_Test2", "IDG")
                //        .WithCronSchedule(withCronSchedule) //dal lun-ven, tutti i mesi alle 8.30
                //        .StartAt(DateTime.UtcNow)
                //        .WithPriority(1)
                //        .Build();

                //IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
                //bool esito = scheduler.CheckExists(trigger.Key);
                //scheduler.UnscheduleJob(trigger.Key);
                //scheduler.Start();
                //scheduler.ScheduleJob(job, trigger);

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


                ISchedulerFactory schedFact = new StdSchedulerFactory();
                IScheduler quartzScheduler = schedFact.GetScheduler();
                quartzScheduler.Start();


                IJobDetail job = JobBuilder.Create<JobFornitori>().WithIdentity(idprocesso + "_" + codist + "JOB_onetime_" + DateTime.Now.Ticks.ToString(), "IDG").Build();
                job.JobDataMap.Add("codist", codist);
                job.JobDataMap.Add("idprocesso", idprocesso);

                DateTime orario = DateTime.Now.AddMinutes(minuti); //valore di polling_inteval
                ITrigger trigger;

                if (minuti == 0)
                {
                    job.JobDataMap.Add("onetime", "true");

                    trigger = TriggerBuilder.Create()
                      .WithIdentity(idprocesso + "_" + codist + "Trigger_onetime_" + DateTime.Now.Ticks.ToString(), "IDG")
                      //.WithIdentity(idprocesso + "_" + codist+"_onetime")
                      .StartNow()
                      //.WithPriority(1)
                      .Build();

                    //Log_Errori.ScriviLOGPROCEDURE("Polling_Interval onetime");
                }
                else
                {
                    job.JobDataMap.Add("onetime", "false");

                    trigger = TriggerBuilder.Create()
                      .WithIdentity(idprocesso + "_" + codist + "_" + DateTime.Now.Ticks.ToString(), "IDG")
                      //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(orario.Hour, orario.Minute)) // execute job daily at 9:30            
                      .StartAt(DateTimeOffset.Now.AddMinutes(minuti))
                      //.StartAt(DateTime.UtcNow)
                      //.WithPriority(1)
                      .Build();

                    //Log_Errori.ScriviLOGPROCEDURE("Polling_Interval "+orario.ToString());
                }

                quartzScheduler.ScheduleJob(job, trigger);
                //quartzScheduler.Start();
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

    [PersistJobDataAfterExecution]
    public class JobFornitori : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                //Log_Errori.ScriviLOGPROCEDURE("JobFornitori execute");

                bool onetime = Convert.ToBoolean(context.JobDetail.JobDataMap.GetString("onetime"));
                LeggiFornitore fornitore = new LeggiFornitore();
                if (onetime)
                {
                    //Log_Errori.ScriviLOGPROCEDURE("JobFornitori execute onetime");

                    fornitore.LetturaFor_ONETIME(
                      context.JobDetail.JobDataMap.GetString("codist"),
                      context.JobDetail.JobDataMap.GetString("idprocesso"));
                }
                else
                {
                    //Log_Errori.ScriviLOGPROCEDURE("JobFornitori execute no_onetime");

                    fornitore.LetturaFor(
                      context.JobDetail.JobDataMap.GetString("codist"),
                      context.JobDetail.JobDataMap.GetString("idprocesso"));
                }

            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }


        }
    }
}




