using Database;
using Database.Base;
using Database.TableInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Web;
#endif
#if NETFRAMEWORK
using System.Web.Services;
#endif
using PollingClass;


namespace PollingClass
{
    /// <summary>
    /// Descrizione di riepilogo per Import_Budget
    /// </summary>
    #if NETFRAMEWORK
    [WebService(Namespace = "http://tempuri.org/")]
#endif
    #if NETFRAMEWORK
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
#endif
    // Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente. 
    // [System.Web.Script.Services.ScriptService]

    public class ImportBudget
#if NETFRAMEWORK
        : System.Web.Services.WebService
#endif
    {
        private dtsTabelle dts = new dtsTabelle();
        DatabaseInfo dbInfo;
        ArrayList array = new ArrayList();

        private string codice_istituto = "";
        private string IdProcesso = "";

        private TaskServiceClass tsk = new TaskServiceClass();

        private DataRow dr;
        private bool OneTime = false;

        public ImportBudget()
        {
        }

        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string ImportaBudget_FirstStart(string codist, string idprocesso)
        {
            return this.ImportaBudget_FirstStart(codist, idprocesso, false);
        }

        public string ImportaBudget_FirstStart(string codist, string idprocesso, bool overrideTask)
        {
            string messaggio = "";
            try
            {
                ImportBudget_Scheduler scheduler = new ImportBudget_Scheduler();
                if (!scheduler.FirstStart(codist, idprocesso, overrideTask))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ImportaBudget_FirstStart errore" + ex.Message);
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;

        }

        public string ImportaBudget_Onetime(string codist, string idprocesso)
        {
            this.OneTime = true;
            string messaggio = "";
            try
            {              

                ImportBudget_Scheduler scheduler = new ImportBudget_Scheduler();
                if (!scheduler.Polling_Interval(0, codist, idprocesso))
                    messaggio = scheduler.messaggio_errore;
            }
            catch (Exception ex)
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ImportaBudget_Onetime errore" + ex.Message);
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;
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
            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "checkfile()");

            this.dbInfo = new DatabaseInfo();
            this.dbInfo.IniziaSessione();
            DatabaseInfo dbinfo2 = new DatabaseInfo(true);
            dbinfo2.IniziaSessione();            

            TableAdapterDimBudgetTemp tbiBudgetTemp = new TableAdapterDimBudgetTemp(dbinfo2, this.dts);

             if (File.Exists(tsk.path_source + @"\" + tsk.file_source))
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "File.Exists");

                //azzero tabella RDA_Esiti_LastOutput_OnLine per istituto 10NN
                Aggset.Eseguicomando("delete DimBudgetTemp where Istituto=" + this.codice_istituto,
                    dbinfo2);
                this.dts.DimBudgetTemp.Rows.Clear();

                tsk.AggiornaStato("4");//metto in elaborazione

                try
                {
                    string pathToExcelFile = tsk.path_source + @"\" + tsk.file_source;
                    ImportaXML importaxls = new ImportaXML();                    
                    List<PollingClass.Budget> bdgs= importaxls.ReadExcelFileDOM(pathToExcelFile);
                    int i = 0;
                    foreach (var result in bdgs)
                    {
                        if (i > 0) //scarto intestazione e inserisco valori tabella
                        {                            
                            //string products = "ProductId : {0}, ProductName: {1}";
                            dr = this.dts.DimBudgetTemp.NewDimBudgetTempRow();
                            dr["Istituto"] = result.IST;
                            dr["CodCDR"] = result.CDR;
                            dr["Materiale"] = result.VOCE;
                            dr["BudgetRilasci"] = Convert.ToDouble( result.BDGRILASCI.Replace(".", ","));
                            dr["BudgetResiduo"] = Convert.ToDouble(result.BDGRESIDUO.Replace(".",","));
                            dr["CodFondo"] = result.CODFONDO;
                            dr["Datarif"] = result.DATARIF;

                            this.dts.DimBudgetTemp.Rows.Add(dr);
                            tbiBudgetTemp.SaveDataTable();
                            tbiBudgetTemp.ConfermaDataset();
                        }
                        i++;
                    }
                    //Console.ReadKey();
                    sp_ImportaBudget.Esegui(this.dbInfo, this.codice_istituto);
                }
                catch (Exception ex)
                {
                    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ImportBudget.CheckFile errore" + ex.Message);
                    PollingExceptionHandler.Esegui(ex);
                }



                // file esistente e nessun dato caricato
                if (this.dts.DimBudgetTemp.Rows.Count == 0)
                {
                    tsk.AggiornaStato("3", "File trovato, ma nessun dato caricato");
                }
                else
                {
                    tsk.AggiornaStato("1");
                }

            }
            else
            {
                Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "File.Exists NO");

                //controllo se ha suoeratoil cutoff
                DateTime orario = Convert.ToDateTime(Convert.ToDateTime(this.dts.TaskService[0]["Orario_Check_File"].ToString()).ToString("HH:mm"));
                Int16 min = Convert.ToInt16(this.dts.TaskService[0]["Cutoff"].ToString());
                DateTime cutoff = orario.AddMinutes(min);
                if (DateTime.Now > cutoff)
                    tsk.AggiornaStato("2", tsk.path_source + " File non trovato in CUT-OFF");    //superato            
                else
                {
                    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "Riprogramma quartz");
                    tsk.AggiornaStato("0");    //NON superato
                    this.ProgrammaQuartz();
                }

            }
            this.dbInfo.TerminaSessione();
            dbinfo2.TerminaSessione();

        }
        //private void CheckFile_old()
        //{
        //    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "checkfile()");

        //    this.dbInfo = new DatabaseInfo();
        //    this.dbInfo.IniziaSessione();
        //    DatabaseInfo dbinfo2 = new DatabaseInfo(true);
        //    dbinfo2.IniziaSessione();

        //    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "dbinfo2.IniziaSessione()");

        //    TableAdapterDimBudgetTemp tbiBudgetTemp = new TableAdapterDimBudgetTemp(dbinfo2, this.dts);

        //    Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "new TableAdapterDimBudgetTemp(dbinfo2, this.dts)");

        //    if (File.Exists(tsk.path_source + @"\" + tsk.file_source))
        //    {
        //        Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "File.Exists");

        //        //azzero tabella RDA_Esiti_LastOutput_OnLine per istituto 10NN
        //        Aggset.Eseguicomando("delete DimBudgetTemp where Istituto=" + this.codice_istituto,
        //            dbinfo2);
        //        this.dts.DimBudgetTemp.Rows.Clear();

        //        tsk.AggiornaStato("4");//metto in elaborazione

        //        try
        //        {
        //            string pathToExcelFile = tsk.path_source + @"\" + tsk.file_source;
        //            ConnexionExcel ConxObject = new ConnexionExcel(pathToExcelFile);
        //            //Query a worksheet with a header row  
        //            var query1 = from a in ConxObject.UrlConnexion.Worksheet<Budget>("BDG") select a;
        //            foreach (var result in query1)
        //            {
        //                //string products = "ProductId : {0}, ProductName: {1}";
        //                dr = this.dts.DimBudgetTemp.NewDimBudgetTempRow();
        //                dr["Istituto"] = result.IST;
        //                dr["CodCDR"] = result.CDR;
        //                dr["Materiale"] = result.VOCE;
        //                dr["BudgetRilasci"] = result.BDGRILASCI;
        //                dr["BudgetResiduo"] = result.BDGRESIDUO;
        //                dr["CodFondo"] = result.CODFONDO;
        //                dr["Datarif"] = result.DATARIF;

        //                this.dts.DimBudgetTemp.Rows.Add(dr);
        //                tbiBudgetTemp.SaveDataTable();
        //                tbiBudgetTemp.ConfermaDataset();
        //            }
        //            //Console.ReadKey();
        //            sp_ImportaBudget.Esegui(this.dbInfo, this.codice_istituto);
        //        }
        //        catch (Exception ex)
        //        {
        //            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ImportBudget.CheckFile errore" + ex.Message);
        //            PollingExceptionHandler.Esegui(ex);
        //        }



        //        // file esistente e nessun dato caricato
        //        if (this.dts.DimBudgetTemp.Rows.Count == 0)
        //        {
        //            tsk.AggiornaStato("3", "File trovato, ma nessun dato caricato");
        //        }
        //        else
        //        {
        //            tsk.AggiornaStato("1");
        //        }

        //    }
        //    else
        //    {
        //        Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "File.Exists NO");

        //        //controllo se ha suoeratoil cutoff
        //        DateTime orario = Convert.ToDateTime(Convert.ToDateTime(this.dts.TaskService[0]["Orario_Check_File"].ToString()).ToString("HH:mm"));
        //        Int16 min = Convert.ToInt16(this.dts.TaskService[0]["Cutoff"].ToString());
        //        DateTime cutoff = orario.AddMinutes(min);
        //        if (DateTime.Now > cutoff)
        //            tsk.AggiornaStato("2", tsk.path_source + " File non trovato in CUT-OFF");    //superato            
        //        else
        //        {
        //            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "Riprogramma quartz");
        //            tsk.AggiornaStato("0");    //NON superato
        //            this.ProgrammaQuartz();
        //        }

        //    }
        //    this.dbInfo.TerminaSessione();
        //    dbinfo2.TerminaSessione();

        //}
        private void ProgrammaQuartz()
        {
            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ProgrammaQuartz");
            //se e' una chiamata singola, non deve riprogrammare il job
            if (this.OneTime)
                return;

            Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "ProgrammaQuartz this.OneTime=false");
            ImportBudget_Scheduler scheduler = new ImportBudget_Scheduler();
            scheduler.Polling_Interval(tsk.polling_interval, this.codice_istituto, this.IdProcesso);
        }


    }

    public class ImportBudget_Scheduler
    {
        public string messaggio_errore = "";
        private readonly IFornitoriSchedulingGateway schedulingGateway;

        public ImportBudget_Scheduler() : this(new ImportBudgetQuartzGateway()) { }

        public ImportBudget_Scheduler(IFornitoriSchedulingGateway schedulingGateway)
        {
            this.schedulingGateway = schedulingGateway ?? throw new ArgumentNullException(nameof(schedulingGateway));
        }

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


                if (!this.schedulingGateway.ScheduleDaily(codist, idprocesso, withCronSchedule, overrideTask))
                {
                    esito = false;
                    Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "checkexists");
                    throw new PollingException(PollingExceptionType.Job_Quartz_esistente, "");
                }
                Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "quartzScheduler.ScheduleJob(job, trigger)");
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
            TaskServiceClass tsk = new TaskServiceClass();
            tsk.LeggiParametri(codist, idprocesso);
            try
            {
                this.schedulingGateway.ScheduleInterval(minuti, codist, idprocesso);
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



}


