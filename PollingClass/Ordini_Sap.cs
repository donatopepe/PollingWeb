using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using System.Web;
#endif
#if NETFRAMEWORK
using System.Web.Services;
#endif
using Database;
using Database.TableInterface;
using Database.Base;
using System.Collections;
using System.Data;
using System.IO;


namespace PollingClass
{

    /// <summary>
    /// Descrizione di riepilogo per OrdiniSAP
    /// </summary>
    #if NETFRAMEWORK
    [WebService(Namespace = "http://tempuri.org/")]
#endif
    #if NETFRAMEWORK
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
#endif
    // Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente. 
    // [System.Web.Script.Services.ScriptService]
    public class Ordini_Sap
#if NETFRAMEWORK
        : System.Web.Services.WebService
#endif
    {

        private dtsTabelle dts = new dtsTabelle();
        DatabaseInfo dbInfo;
        ArrayList array = new ArrayList();

        private string codice_istituto = "";
        private string codice_guid = "";

        private string IdProcesso = "";

        private bool bInviofile = false;
        private string sTipoexport;
        private TaskServiceClass tsk = new TaskServiceClass();

        private DataRow dr;
        private bool OneTime = false;

        public Ordini_Sap()
        {
        }

        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string ScriviRDA(string istituto)
        {
            //this.codice_istituto = codist;
            this.codice_istituto = istituto;

            this.dbInfo = new DatabaseInfo();
            this.dbInfo.IniziaSessione();

            //this.LeggiParametri(codice_istituto, idprocesso);
            this.ScriviRiga_RDA();

            this.dbInfo.TerminaSessione();
            return "eseguito";
        }


        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string ScriviOrdini(string ist)
        {
            this.codice_istituto = ist;
            //this.codice_guid = guid;

            this.dbInfo = new DatabaseInfo();
            this.dbInfo.IniziaSessione();


            //this.LeggiParametri(codice_istituto, idprocesso);
            this.ScriviRiga_ORDINI();

            this.dbInfo.TerminaSessione();
            return "eseguito";
        }


        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string Invia_file_Sap_FirstStart(string codist, string idprocesso,string tipoExport)
        {
            return this.Invia_file_Sap_FirstStart(codist, idprocesso, false, tipoExport);
        }

        public string Invia_file_Sap_FirstStart(string codist, string idprocesso, bool overrideTask, string tipoExport)
        {
            string messaggio = "";
            try
            {
                Ordini_Scheduler scheduler = new Ordini_Scheduler();
                if (!scheduler.FirstStart_Invio(codist, idprocesso, overrideTask, tipoExport))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;


            //return this.Invia_file_Sap(codist, idprocesso, false);        
        }


        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string Invia_file_Sap_Onetime(string codist, string idprocesso,string tipoExport)
        {
            string messaggio = "";
            try
            {
                Ordini_Scheduler scheduler = new Ordini_Scheduler();
                if (!scheduler.Polling_Interval_Invio(0, codist, idprocesso, tipoExport))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                messaggio = PollingExceptionHandler.Esegui(ex);

            }
            return messaggio;
            //return this.Invia_file_Sap(codist, idprocesso, true);
        }


        public string Invia_file_Sap(string codist, string idprocesso, bool onetime, string tipoExport)
        {
            this.codice_istituto = codist;
            this.IdProcesso = idprocesso;
            this.OneTime = onetime;
            this.bInviofile = true;
            this.sTipoexport = tipoExport;

            this.dts = tsk.LeggiParametri(codice_istituto, idprocesso);
            //this.LeggiParametri(codice_istituto, idprocesso);

            if (this.dts.LogTaskService.Rows.Count > 0)
                if (this.dts.LogTaskService[0]["Codice_stato"].ToString() == "0")
                this.Crea_file_Sap();

            return "eseguito";
        }


        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string Leggi_Esito_Sap_FirstStart(string codist, string idprocesso)
        {
            return this.Leggi_Esito_Sap_FirstStart(codist, idprocesso, false);
        }

        public string Leggi_Esito_Sap_FirstStart(string codist, string idprocesso, bool overrideTask)
        {
            string messaggio = "";
            try
            {
                Ordini_Scheduler scheduler = new Ordini_Scheduler();
                if (!scheduler.FirstStart_Esito(codist, idprocesso, overrideTask))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;


        }

        #if NETFRAMEWORK
        [WebMethod]
#endif
        public string Leggi_Esito_Sap_Onetime(string codist, string idprocesso)
        {
            //return this.Leggi_Esito_Sap(codist, idprocesso, true);        
            string messaggio = "";
            try
            {
                Ordini_Scheduler scheduler = new Ordini_Scheduler();
                if (!scheduler.Polling_Interval_Esito(0, codist, idprocesso))
                    messaggio = scheduler.messaggio_errore;

            }
            catch (Exception ex)
            {
                messaggio = PollingExceptionHandler.Esegui(ex);
            }
            return messaggio;
        }

        public string Leggi_Esito_Sap(string codist, string idprocesso, bool onetime)
        {
            this.codice_istituto = codist;
            this.IdProcesso = idprocesso;
            this.OneTime = onetime;
            this.bInviofile = false;
            this.sTipoexport = "";

            this.dts = tsk.LeggiParametri(codice_istituto, idprocesso);
            //this.LeggiParametri(codice_istituto, idprocesso);

            if (this.dts.LogTaskService.Rows.Count > 0)
                if (this.dts.LogTaskService[0]["Codice_stato"].ToString() == "0")
                this.CheckFile();

            return "eseguito";
        }

        private void ScriviRiga_RDA()
        {
            try
            {
                //controllo che in caso di timeout si sia chiusa la connessione
                if (this.dbInfo.connessione.State == ConnectionState.Closed)
                    this.dbInfo.IniziaSessione();

                //inserimento/aggiornamento anagrafica
                sp_Flusso_RDA_ist.Esegui(this.dbInfo, this.codice_istituto);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);            
                PollingExceptionHandler.Esegui(ex);
            }
        }

        private void ScriviRiga_ORDINI()
        {
            try
            {
                //controllo che in caso di timeout si sia chiusa la connessione
                if (this.dbInfo.connessione.State == ConnectionState.Closed)
                    this.dbInfo.IniziaSessione();

                //inserimento/aggiornamento anagrafica
                //sp_Flusso_Ordini_SAP.Esegui(this.dbInfo, this.codice_guid);
                sp_Flusso_Ordini_SAP_ist.Esegui(this.dbInfo,  this.codice_istituto);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);            
                PollingExceptionHandler.Esegui(ex);
            }
        }

        private void Crea_file_Sap()
        {
            DatabaseInfo dbinfo = new DatabaseInfo();
            DatabaseInfo dbinfo2 = new DatabaseInfo(true);
            try
            {
                
                dbinfo.IniziaSessione();                
                dbinfo2.IniziaSessione();

                //this.dbInfo = new DatabaseInfo();
                //this.dbInfo.IniziaSessione();



                FileInfo file_sap = new FileInfo(tsk.path_source + @"\" + tsk.file_source);
                if (file_sap.Exists)
                {
                    //implementare se bisogna creare un alert di elaborazione??
                    //file_sap.Delete();
                    tsk.AggiornaStato("3", "file invio oda ancora presente su server");
                    throw new PollingException(PollingExceptionType.file_presente_nella_cartella, "file oda presente");
                }

                //popolo tabella Tbl_Export_SAP_OnLine
                if (this.IdProcesso.IndexOf("Ordini")>=0)
                    sp_Flusso_Ordini_SAP_ist.Esegui(dbinfo, this.codice_istituto);
                else
                    sp_Flusso_RDA_ist.Esegui(dbinfo, this.codice_istituto);


                TableAdapterTbl_Export_SAP_OnLine tbiExport = new TableAdapterTbl_Export_SAP_OnLine(dbinfo2, this.dts);
                

                ArrayList array = new ArrayList();
                FiltroCampi fc;
                fc = new FiltroCampi();
                fc.Campo = "Codist";
                fc.Valore = this.codice_istituto;
                fc.Tipo = "N";
                array.Add(fc);
                
                fc = new FiltroCampi();
                fc.Campo = "Tiporecord";
                fc.Valore = this.sTipoexport;
                fc.Tipo = "N";
                array.Add(fc);

                fc = new FiltroCampi();
                fc.Campo = "isnull(Elaborato,0)";
                fc.Valore = "0";
                fc.Tipo = "N";
                array.Add(fc);
                tbiExport.Load(array);

                if (this.dts.Tbl_Export_SAP_OnLine.Rows.Count > 0)
                    tsk.AggiornaStato("4");

                for (int i = 0; i < this.dts.Tbl_Export_SAP_OnLine.Rows.Count; i++)
                {

                    using (StreamWriter streamwriter = new StreamWriter(file_sap.FullName, append: true))
                    {
                        streamwriter.WriteLine(dts.Tbl_Export_SAP_OnLine.Rows[i]["Riga"].ToString());

                        this.dts.Tbl_Export_SAP_OnLine.Rows[i].BeginEdit();
                        this.dts.Tbl_Export_SAP_OnLine.Rows[i]["Elaborato"] = 1;
                        this.dts.Tbl_Export_SAP_OnLine.Rows[i].EndEdit();
                    }

                }
                tbiExport.SaveDataTable();
                tbiExport.ConfermaDataset();

                //aggiorna log
                file_sap = new FileInfo(tsk.path_source + @"\" + tsk.file_source);
                if (file_sap.Exists)
                    Aggset.Eseguicomando("INSERT INTO eProcurement.dbo.Log_ODA_SAP (Istituto, Data_Put, Nrighe_input, Size_input) " +
                                        "VALUES(" + this.codice_istituto + "," +
                                        "getdate()," +
                                        this.dts.Tbl_Export_SAP_OnLine.Rows.Count + ", " +
                                        file_sap.Length + ")",
                                        dbinfo2);

                //cancella righe da Tbl_Export_SAP_OnLine gia' elaborate per istituto
                Aggset.Eseguicomando("delete eProcurement.dbo.Tbl_Export_SAP_OnLine where Elaborato=1 and Codist=" + this.codice_istituto,
                                        dbinfo2);

                //Aggiorna Stato LogTaskService
                if (this.dts.Tbl_Export_SAP_OnLine.Rows.Count > 0)
                    tsk.AggiornaStato("1"); //il file e' stato creato con righe
                else
                    tsk.AggiornaStato("3", "file non creato. nessuna riga oda presente"); //il file e' stato creato con righe

                //this.dbInfo.TerminaSessione();
                dbinfo.TerminaSessione();
                dbinfo2.TerminaSessione();
            }
            catch (Exception ex)
            {
                //if (this.dbInfo.connessione.State == ConnectionState.Open)
                //    this.dbInfo.TerminaSessione();
                if (dbinfo2.connessione.State == ConnectionState.Open)
                    dbinfo2.TerminaSessione();

                PollingExceptionHandler.Esegui(ex);
            }


        }


        private void CheckFile()
        {
            DateTime datafile;
            this.dbInfo = new DatabaseInfo();
            this.dbInfo.IniziaSessione();

            DatabaseInfo dbinfo2 = new DatabaseInfo(true);
            dbinfo2.IniziaSessione();

            TableAdapterRDA_Esiti_LastOutput_OnLine tbiEsiti = new TableAdapterRDA_Esiti_LastOutput_OnLine(dbinfo2, this.dts);

            //if (File.Exists(this.dts.TaskService[0].Path_Source))
            if (File.Exists(tsk.path_source + @"\" + tsk.file_source))
            {
                //azzero tabella RDA_Esiti_LastOutput_OnLine per istituto 10NN
                Aggset.Eseguicomando("delete RDA_Esiti_LastOutput_OnLine where Societa='10" + this.codice_istituto + "'",
                    dbinfo2);
                this.dts.RDA_Esiti_LastOutput_OnLine.Rows.Clear();

                datafile = File.GetLastWriteTime(tsk.path_source + @"\" + tsk.file_source);
                tsk.AggiornaStato("4");//metto in elaborazione
                var line = "";
                var values = new string[0];
                using (var leggi = new StreamReader(tsk.path_source + @"\" + tsk.file_source))
                {
                    //leggo il file
                    while (!leggi.EndOfStream)
                    {
                        line = leggi.ReadLine().Trim();

                        dr = this.dts.RDA_Esiti_LastOutput_OnLine.NewRDA_Esiti_LastOutput_OnLineRow();
                        dr["Mandante"] = line.Substring(0, 3);
                        dr["Societa"] = line.Substring(3, 4);
                        dr["Num_Ordine"] = line.Substring(7, 10);
                        dr["Posizione"] = line.Substring(17, 3);
                        dr["Data_FileOutput"] = datafile;
                        if (line.Length > 20)
                        {
                            dr["Cod_Errore"] = line.Substring(20, 5);
                            dr["Desc_Errore"] = line.Substring(25, line.Length - 25);

                            //implementare 
                            //invio messaggio singolo di errore esito?
                        }
                        this.dts.RDA_Esiti_LastOutput_OnLine.Rows.Add(dr);
                        tbiEsiti.SaveDataTable();
                        tbiEsiti.ConfermaDataset();

                    }
                    leggi.Close();

                    //inserimento/aggiornamento anagrafica
                    sp_Esiti_Ordini_SAP.Esegui(this.dbInfo);

                    //aggiorna log
                    FileInfo file_sap = new FileInfo(tsk.path_source + @"\" + tsk.file_source);
                    Aggset.Eseguicomando(
                        "UPDATE Log_ODA_SAP SET " +
                        "Data_Get = Convert(DateTime, '" + file_sap.LastWriteTime + "',103)," +
                        "Nrighe_output = " + this.dts.RDA_Esiti_LastOutput_OnLine.Count.ToString() + "," +
                        "Size_output = " + file_sap.Length.ToString() +
                        " WHERE(ID = (SELECT TOP(1) ID FROM Log_ODA_SAP WHERE(Istituto = " + this.codice_istituto + ")  ORDER BY ID DESC)) AND(Data_Get IS NULL) AND(Istituto = " + this.codice_istituto + ") ",
                        dbinfo2);

                    // file esistente e nessun dato caricato
                    if (this.dts.RDA_Esiti_LastOutput_OnLine.Rows.Count == 0)
                    {
                        tsk.AggiornaStato("3", "File trovato, ma nessun dato caricato");
                    }
                    else
                    {
                        tsk.AggiornaStato("1");
                    }
                }

            }
            else
            {
                //controllo se ha suoeratoil cutoff
                DateTime orario = this.dts.TaskService[0].Orario_Check_File;
                Int16 min = Convert.ToInt16(this.dts.TaskService[0]["Cutoff"].ToString());
                DateTime cutoff = orario.AddMinutes(min);
                if (DateTime.Now > cutoff)
                    tsk.AggiornaStato("2", tsk.path_source + " File non trovato in CUT-OFF");    //superato            
                else
                {
                    tsk.AggiornaStato("0");    //NON superato
                    this.ProgrammaQuartz();
                }

            }
            this.dbInfo.TerminaSessione();
            dbinfo2.TerminaSessione();
        }


        private void ProgrammaQuartz()
        {
            //se e' una chiamata singola, non deve riprogrammare il job
            if (this.OneTime)
                return;

            Ordini_Scheduler ordini = new Ordini_Scheduler();
            if (this.bInviofile)
            {
                ordini.Polling_Interval_Invio(tsk.polling_interval, this.codice_istituto, this.IdProcesso,this.sTipoexport);
            }
            else
            {
                ordini.Polling_Interval_Esito(tsk.polling_interval, this.codice_istituto, this.IdProcesso);
            }


        }
    }

    public class Ordini_Scheduler
    {
        public string messaggio_errore = "";
        private readonly IOrdiniSchedulingGateway schedulingGateway;

        public Ordini_Scheduler() : this(new OrdiniQuartzGateway()) { }

        public Ordini_Scheduler(IOrdiniSchedulingGateway schedulingGateway)
        {
            this.schedulingGateway = schedulingGateway ?? throw new ArgumentNullException(nameof(schedulingGateway));
        }
        public bool _FirstStart_Invio(string codist, string idprocesso)
        {
            return this.FirstStart(codist, idprocesso, true, false,"");
        }
        public bool FirstStart_Invio(string codist, string idprocesso, bool overrideTask, string tipoExport)
        {
            return this.FirstStart(codist, idprocesso, true, overrideTask, tipoExport);
        }

        public bool FirstStart_Esito(string codist, string idprocesso, bool overrideTask)
        {
            return this.FirstStart(codist, idprocesso, false, overrideTask,"");
        }

        //private bool _FirstStart(string codist, string idprocesso, bool InvioFile)
        //{
        //    return _FirstStart(codist, idprocesso, InvioFile, false);
        //}

        private bool FirstStart(string codist, string idprocesso, bool InvioFile, bool overrideTask, string tipoExport)
        {
            bool esito = true;
            dtsTabelle dts = new dtsTabelle();
            TaskServiceClass tsk = new TaskServiceClass();
            dts = tsk.LeggiParametri(codist, idprocesso);
            

            string orario_start = "0 8"; //8 per default
            string day_start = "MON-FRI";
            string withCronSchedule = "0 30 8 ? * MON-FRI";
            if (dts.TaskService.Rows.Count == 1)
            {
                var configuredTime = (DateTime)dts.TaskService.Rows[0]["Orario_Check_File"];
                orario_start = configuredTime.Minute.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " +
                               configuredTime.Hour.ToString(System.Globalization.CultureInfo.InvariantCulture);

                day_start = dts.TaskService.Rows[0]["Day_Check_File"].ToString();
                withCronSchedule = "0 " + orario_start + " ? * " + day_start.Trim();
            }
            //dbInfo.TerminaSessione();

            try
            {

                if (!this.schedulingGateway.ScheduleDaily(codist, idprocesso, InvioFile,
                    tipoExport, FornitoriSchedulePolicy.WeekdayCronFromLegacy(withCronSchedule), overrideTask))
                {
                    esito = false;
                    throw new PollingException(PollingExceptionType.Job_Quartz_esistente, "");
                }


            }
            catch (PollingException ex)
            {
                messaggio_errore = PollingExceptionHandler.Esegui(ex);
            }

            return esito;
        }

        



        public bool Polling_Interval_Invio(int minuti, string codist, string idprocesso,string tipoexport)
        {
            return this.Polling_Interval(minuti, codist, idprocesso, true,tipoexport );
        }
        public bool Polling_Interval_Esito(int minuti, string codist, string idprocesso)
        {
            return this.Polling_Interval(minuti, codist, idprocesso, false,"");
        }
        private bool Polling_Interval(int minuti, string codist, string idprocesso, bool inviofile,string tipoexport)
        {
            bool esito = true;
            dtsTabelle dts = new dtsTabelle();
            TaskServiceClass tsk = new TaskServiceClass();
            dts = tsk.LeggiParametri(codist, idprocesso);
            try
            {

                this.schedulingGateway.ScheduleInterval(minuti, codist, idprocesso, inviofile, tipoexport);
            }
            catch (Exception ex)
            {
                esito = false;
                messaggio_errore = PollingExceptionHandler.Esegui(ex);
            }

            return esito;
        }

        
    }


}