using Database;
using Database.Base;
using Database.TableInterface;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollingClass
{
    public class ExportSQL01
    {
        private dtsTabelle dts = new dtsTabelle();
        DatabaseInfo dbInfo;
        ArrayList array = new ArrayList();

        private string codice_istituto = "";
        private string IdProcesso = "";

        private TaskServiceClass tsk = new TaskServiceClass();

        private DataRow dr;
        private bool OneTime = false;

        public string SQL01_FirstStart(string codist, string idprocesso)
        {
            return this.SQL01_FirstStart(codist, idprocesso, false);
        }

        public string SQL01_FirstStart(string codist, string idprocesso, bool overrideTask)
        {
            string messaggio = "";
            try
            {
                SQL01_Scheduler scheduler = new SQL01_Scheduler();
                if (!scheduler.FirstStart(codist, idprocesso, overrideTask))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "SQL01_FirstStart errore" + ex.Message);
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;

        }


        public string SQL01_Onetime(string codist, string idprocesso)
        {
            this.OneTime = true;
            string messaggio = "";
            try
            {

                SQL01_Scheduler scheduler = new SQL01_Scheduler();
                if (!scheduler.Polling_Interval(0, codist, idprocesso))
                    messaggio = scheduler.messaggio_errore;
            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "SQL01_Onetime errore" + ex.Message);
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;
        }


        public class SQL01_Scheduler
        {
            public string messaggio_errore = "";

            public bool FirstStart(string codist, string idprocesso, bool overrideTask)
            {
                Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "firststart");

                bool esito = true;
                dtsTabelle dts = new dtsTabelle();
                TaskServiceClass tsk = new TaskServiceClass();
                dts = tsk.LeggiParametri(codist, idprocesso);

                try
                {
                    int min_start, ora_start;
                    min_start = 0;
                    ora_start = 0;
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


                    ISchedulerFactory schedFact = new StdSchedulerFactory();
                    IScheduler quartzScheduler = schedFact.GetScheduler();
                    //var workingDays = new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

                    quartzScheduler.Start();

                    IJobDetail job = JobBuilder.Create<JobSQL01>()
                            .WithIdentity("JOB_" + idprocesso + "_" + codist, "IDG")
                            .Build();

                    job.JobDataMap.Add("codist", codist);
                    job.JobDataMap.Add("idprocesso", idprocesso);

                   
                    ITrigger trigger = TriggerBuilder.Create()
                     .WithIdentity("Trigger_" + idprocesso + "_" + codist, "IDG")
                     .WithCronSchedule(withCronSchedule) //dal lun-ven, tutti i mesi alle 8.30
                     //.WithSchedule(CronScheduleBuilder.AtHourAndMinuteOnGivenDaysOfWeek(ora_start, min_start, workingDays))
                     .Build();


                    //cancello trigger
                    if (overrideTask)
                        quartzScheduler.UnscheduleJob(trigger.Key);

                    if (quartzScheduler.CheckExists(trigger.Key))
                    {
                        esito = false;
                        Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "checkexists");
                        throw new PollingException(PollingExceptionType.Job_Quartz_esistente, "");
                    }

                    quartzScheduler.ScheduleJob(job, trigger);
                    Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "quartzScheduler.ScheduleJob(job, trigger)");
                }
                catch (PollingException ex)
                {
                    Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "errore  FirstStart " + ex.Message);
                    messaggio_errore = PollingExceptionHandler.Esegui(ex);
                }

                return esito;
            }

            public bool Polling_Interval(int minuti, string codist, string idprocesso)
            {
                Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "polling_interval");

                bool esito = true;
                dtsTabelle dts = new dtsTabelle();
                TaskServiceClass tsk = new TaskServiceClass();
                string withIdentity = idprocesso + "_" + codist + "JOB_onetime_" + DateTime.Now.Ticks.ToString();
                dts = tsk.LeggiParametri(codist, idprocesso);
                try
                {
                    ISchedulerFactory schedFact = new StdSchedulerFactory();
                    IScheduler quartzScheduler = schedFact.GetScheduler();
                    quartzScheduler.Start();

                    IJobDetail job = JobBuilder.Create<JobSQL01>()
                            //.WithIdentity("JOB_" + idprocesso + "_" + codist, "IDG")
                            .WithIdentity(withIdentity, "IDG")
                            .Build();

                    job.JobDataMap.Add("codist", codist);
                    job.JobDataMap.Add("idprocesso", idprocesso);

                    DateTime orario = DateTime.Now.AddMinutes(minuti);//valore di polling_inteval
                    ITrigger trigger;

                    if (minuti == 0)
                    {
                        job.JobDataMap.Add("onetime", "true");

                        trigger = TriggerBuilder.Create()
                            .WithIdentity(withIdentity, "IDG")
                            .StartNow()
                            //.WithPriority(1)
                            .Build();

                        Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "trigger onetime");
                    }
                    else
                    {
                        job.JobDataMap.Add("onetime", "false");

                        trigger = TriggerBuilder.Create()
                            .WithIdentity(withIdentity, "IDG")
                            .StartAt(DateTimeOffset.Now.AddMinutes(minuti))
                            .Build();
                        Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "trigger no onetime");
                    }

                    quartzScheduler.ScheduleJob(job, trigger);
                    Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "quartzScheduler.ScheduleJob(job, trigger)");
                }
                catch (Exception ex)
                {
                    Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "errore polling_interval " + ex.Message);
                    esito = false;
                    messaggio_errore = PollingExceptionHandler.Esegui(ex);
                }

                return esito;
            }

        }

        public string Leggi_File(string codist, string idprocesso, bool onetime)
        {
            this.codice_istituto = codist;
            this.IdProcesso = idprocesso;
            this.OneTime = onetime;

            this.dts = tsk.LeggiParametri(codice_istituto, idprocesso);

            if (this.dts.LogTaskService.Rows.Count > 0)
                if (this.dts.LogTaskService[0]["Codice_stato"].ToString() == "0")
                this.CheckFile();

            return "eseguito";
        }


        private void CheckFile()
        {
            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "EXPORT SQL01 checkfile()");

            this.dbInfo = new DatabaseInfo();
            this.dbInfo.IniziaSessione();
            DatabaseInfo dbinfo2 = new DatabaseInfo(true);
            dbinfo2.IniziaSessione();

            SqlDataReader reader = null;
            string rigafile = "";
            int totrighe = 0;
            
            try
            {
                FileInfo file_sql01 = new FileInfo(tsk.path_source + @"\" + tsk.file_source);
                if (file_sql01.Exists)
                {
                    File.Delete(tsk.path_source + @"\" + tsk.file_source);
                }

                
                //reader = Aggset.Read(sSql, "", sCampi, dbinfo2);
                reader = Aggset.Read("ViewEstrazioneRdARdE", "", "*", dbinfo2);
                

                tsk.AggiornaStato("4");//metto in elaborazione

                //VERSIONE CORRETTA CON |
                //using (StreamWriter streamwriter = new StreamWriter(file_sql01.FullName, append: true))
                //{
                //    while (reader.Read())
                //    {
                //        rigafile = "";
                //        for (int i=0; i< reader.FieldCount; i++)                        
                //            rigafile = rigafile + reader[i].ToString() + "|";

                //        streamwriter.WriteLine(rigafile);            
                //    }
                //}

                //VERSIONE OPENXML               
                SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                    Create(tsk.path_source + @"\" + tsk.file_source, SpreadsheetDocumentType.Workbook);

                // Add a WorkbookPart to the document.
                WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();

                SheetData sheetData = new SheetData();

                // Add Sheets to the Workbook.
                Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                    AppendChild<Sheets>(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet()
                {
                    Id = spreadsheetDocument.WorkbookPart.
                    GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "DemoPolling"
                };
                sheets.Append(sheet);
                workbookpart.Workbook.Save();


                //Row row = new Row() { RowIndex = 1 };
                //// Create new cell
                //Cell cell = new Cell()                
                //{ CellReference = "A1", DataType = CellValues.String, CellValue = new CellValue("valoretest") };
                //// Append cell to row
                //row.Append(cell);

                Row row;
                Cell cell;

                //lettura colonne
                row = new Row();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(reader.GetName(i).ToString());
                    row.AppendChild(cell);
                }
                sheetData.Append(row);

                //lettura valori
                totrighe = 0;
                while (reader.Read())
                {
                    totrighe = totrighe + 1;
                    row = new Row();
                    rigafile = "";
                    for (int i = 0; i < reader.FieldCount; i++)
                    {                        
                        cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(reader[i].ToString());
                        row.AppendChild(cell);
                    }
                    sheetData.Append(row);
                }

                

                //cell = new Cell();
                //cell.DataType = CellValues.String;
                //cell.CellValue = new CellValue("Test01");
                //row.AppendChild(cell);
                //cell = new Cell();
                //cell.DataType = CellValues.String;
                //cell.CellValue = new CellValue("Test02");
                //row.AppendChild(cell);

                
                //// Append row to sheetData
                //sheetData.Append(row);
                // Append sheetData to worksheet
                worksheetPart.Worksheet = new Worksheet(sheetData);
                //worksheetPart.Worksheet.Append(sheetData);
                worksheetPart.Worksheet.Save();

                // Close the document.
                spreadsheetDocument.Close();
                
                // file esistente e nessun dato caricato
                if (totrighe == 0)
                {
                    tsk.AggiornaStato("3", "nessun dato caricato");
                }
                else
                {
                    tsk.AggiornaStato("1");
                }
            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ExportSQL01 " + ex.Message);

                if (dbinfo2.connessione.State == ConnectionState.Open)
                    dbinfo2.TerminaSessione();
                if (!reader.IsClosed)
                    reader.Close();

                PollingExceptionHandler.Esegui(ex);
            }

                        
            //if (File.Exists(tsk.path_source + @"\" + tsk.file_source))
            //{
                                          

            //}
            //else
            //{
            //    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "File.Exists NO");

            //    //controllo se ha suoeratoil cutoff
            //    DateTime orario = Convert.ToDateTime(Convert.ToDateTime(this.dts.TaskService[0]["Orario_Check_File"].ToString()).ToString("HH:mm"));
            //    Int16 min = Convert.ToInt16(this.dts.TaskService[0]["Cutoff"].ToString());
            //    DateTime cutoff = orario.AddMinutes(min);
            //    if (DateTime.Now > cutoff)
            //        tsk.AggiornaStato("2", tsk.path_source + " File non trovato in CUT-OFF");    //superato            
            //    else
            //    {
            //        Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "Riprogramma quartz");
            //        tsk.AggiornaStato("0");    //NON superato
            //        this.ProgrammaQuartz();
            //    }

            //}
            this.dbInfo.TerminaSessione();
            dbinfo2.TerminaSessione();

        }

        


    }

    [PersistJobDataAfterExecution]
    public class JobSQL01 : IJob
    {
        public void Execute(IJobExecutionContext context)
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
        }
    }
}
