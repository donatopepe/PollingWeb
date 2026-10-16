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


using PollingClass;

namespace PollingClass
{
  /// <summary>
  /// Descrizione di riepilogo per ImportFatture
  /// </summary>
  #if NETFRAMEWORK
    [WebService(Namespace = "http://tempuri.org/")]
#endif
  #if NETFRAMEWORK
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
#endif
// Per consentire la chiamata di questo servizio Web dallo script utilizzando ASP.NET AJAX, rimuovere il commento dalla riga seguente. 
// [System.Web.Script.Services.ScriptService]
  public class ImportFatture
#if NETFRAMEWORK
  : System.Web.Services.WebService
#endif
  {
    //private TableAdapterLogTaskService tbiLogTaskService;
    //private TableAdapterTaskService tbiTaskService;
    private dtsTabelle dts = new dtsTabelle();
    DatabaseInfo dbInfo;

    private string msgmail_istituto_processo = "";

    private string codice_istituto = "";

    //private int polling_interval = 10; //tempo default
    //private string mail_failure = "";
    //private string mail_success = "";
    //private string archivia_path = "";
    //private string path_source = "";
    //private string file_source = "";
    //private string mail_mittente = "";
    private string IdProcesso = "";
    private bool OneTime = false;
    private TaskServiceClass tsk = new TaskServiceClass();

    ArrayList array = new ArrayList();
    //FiltroCampi fc;

    public ImportFatture()
    {
    }




    #if NETFRAMEWORK
        [WebMethod]
#endif
    public string FirstStart(string codist, string idprocesso)
    {
      return FirstStart(codist, idprocesso, false);
    }

    public string FirstStart(string codist, string idprocesso, bool overrideTask)
    {
      string messaggio = "";
      try
      {
        //Log_Errori.ScriviLOGPROCEDURE("FirstStart ");

        ImportFatture_Scheduler scheduler_for = new ImportFatture_Scheduler();
        if (!scheduler_for.FirstStart(codist, idprocesso, overrideTask))
          messaggio = scheduler_for.messaggio_errore;
        else
        {
          //this.LeggiParametri(codice_istituto, idprocesso);
          //this.AggiornaStato("0", "start servizio");
        }

      }
      catch (Exception ex)
      {
        Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "FirstStart errore" + ex.Message);
        PollingExceptionHandler.Esegui(ex);
      }

      return messaggio;
    }


    #if NETFRAMEWORK
        [WebMethod]
#endif
    public string ONETIME_nuovo(string codist, string idprocesso)
    {
      string messaggio = "";
      try
      {
        this.dts = tsk.LeggiParametri(codist, idprocesso);

        if (this.dts.LogTaskService[0]["Codice_stato"].ToString() != "0")
        {
          throw new PollingException(PollingExceptionType.Task_in_esecuzione,
            "stato " + this.dts.LogTaskService[0]["Codice_stato"].ToString());
        }



        ImportFatture_Scheduler scheduler_for = new ImportFatture_Scheduler();
        if (!scheduler_for.Polling_Interval(0, codist, idprocesso))
          messaggio = scheduler_for.messaggio_errore;
        else
        {
          //this.LeggiParametri(codice_istituto, idprocesso);
          //this.AggiornaStato("0", "start servizio");
        }

      }
      catch (Exception ex)
      {
        Log_Errori.ScriviLOGPROCEDURE(codist, idprocesso, "FirstStartImportFatture_onetime errore" + ex.Message);
        messaggio = PollingExceptionHandler.Esegui(ex);
      }

      return messaggio;


    }



    public void Lettura(string codist, string idprocesso)
    {
      //Log_Errori.ScriviLOGPROCEDURE("LetturaFor start");
      this.msgmail_istituto_processo = "SCHEDULATA: (" + codist + " " + idprocesso + ")";
      this.codice_istituto = codist;
      this.IdProcesso = idprocesso;

      this.dts = tsk.LeggiParametri(this.codice_istituto, this.IdProcesso);
      if (this.dts.LogTaskService.Rows.Count>0)
        if (this.dts.LogTaskService[0]["Codice_stato"].ToString() == "0")
            this.CheckFile();

      //Log_Errori.ScriviLOGPROCEDURE("LetturaFor fine");

    }

    public void Lettura_ONETIME(string codist, string idprocesso)
    {
      this.codice_istituto = codist;
      this.IdProcesso = idprocesso;
      this.OneTime = true;
      this.msgmail_istituto_processo = "ONETIME: Istituto=" + this.codice_istituto + " - Processo=" + this.IdProcesso;

      this.dts = tsk.LeggiParametri(this.codice_istituto, this.IdProcesso);
            //this.LeggiParametri(codist, idprocesso,onetime);
            if (this.OneTime &&
          this.dts.LogTaskService[0]["Codice_stato"].ToString() != "4")
      {
        this.CheckFile();
      }
      else if (this.dts.LogTaskService[0]["Codice_stato"].ToString() == "0")
      {
        this.CheckFile();
      }

    }


    private void CheckFile()
    {
      //Log_Errori.ScriviLOGPROCEDURE("CheckFile ");
      bool esisteRiga = false;
      try
      {
        this.dbInfo = new DatabaseInfo();
        this.dbInfo.IniziaSessione();

        //if (File.Exists(this.dts.TaskService[0].Path_Source))
        if (File.Exists(ConfiguredFilePaths.Combine(tsk.path_source, tsk.file_source)))
        {
          ////Log_Errori.ScriviLOGPROCEDURE("CheckFile trovatofile ");
          //try
          //{
          //  //azzero target per istituto
          //  AzzeraTargetSC00G100.Esegui(this.dbInfo, this.codice_istituto);
          //}
          //catch (Exception ex)
          //{
          //  Console.WriteLine(ex.Message);
          //  PollingExceptionHandler.Esegui(ex);
          //  //this.AggiornaStato("3", ex.Message);
          //}

          //TableAdapterTargetSC00G100 tbiTarget = new TableAdapterTargetSC00G100(this.dbInfo, this.dts);
          //DataRow dr;

          ////Log_Errori.ScriviLOGPROCEDURE("CheckFile stato 4 ");

          tsk.AggiornaStato("4"); //metto in elaborazione
                                  //var line = "";
                                  //var values = new string[0];
                                  //{
                                  //  //azzero tabella
                                  //  this.dts.TargetSC00G100.Rows.Clear();

                    //  //leggo il file
                    //  while (!leggi.EndOfStream)
                    //  {
                    //    esisteRiga = false;

                    //    line = leggi.ReadLine();

                    //    if (line.Split('|').Length == 8
                    //    ) //1001  |UMB. CECCARELLI GENERAL TRADE SRL  |P.ZZA SALLUSTIO, 13/14/15          |RM |00187   |ROMA                            |07506810584     |                        
                    //      line = "|" + line;

                    //    values = line
                    //      .Split('|'); //|1001  |UMB. CECCARELLI GENERAL TRADE SRL  |P.ZZA SALLUSTIO, 13/14/15          |RM |00187   |ROMA                            |07506810584     |
                    //    if (values.Length == 9)
                    //      esisteRiga = true;
                    //    else
                    //    {
                    //      if (line.Contains("�"))
                    //        line = "�" + line; //fix per avere stesse colonne di |
                    //      values = line
                    //        .Split('�'); //Ò³1001  ³COMUNE DI DELICETO                 ³CORSO MARGHERITA 45                ³FG ³71026   ³DELICETO                           ³80003310713     ³
                    //      if (values.Length == 9)
                    //      {
                    //        //fix campo codice                                
                    //        values[1] = values[1].Substring(1);
                    //        esisteRiga = true;
                    //      }

                    //    }

                    //    if (esisteRiga)
                    //    {
                    //      if (IsNumeric.Esegui(values[1]))
                    //      {
                    //        try
                    //        {
                    //          //rigaletta += 1;
                    //          //Inserisci riga in tabella
                    //          dr = this.dts.TargetSC00G100.NewTargetSC00G100Row();
                    //          dr["Istituto"] = this.codice_istituto;
                    //          dr["Cfor"] = values[1].Trim();
                    //          dr["Ragione_Sociale"] = values[2].Trim();
                    //          dr["Indirizzo"] = values[3].Trim();
                    //          dr["Prov"] = values[4].Trim();
                    //          dr["Cap"] = values[5].Trim();
                    //          dr["Citta"] = values[6].Trim();

                    //          if (values[7].Trim().Length == 11)
                    //          {
                    //            dr["Piva"] = values[7].Trim();
                    //            dr["Codfis"] = "";
                    //          }
                    //          else
                    //          {
                    //            dr["Codfis"] = values[7].Trim();
                    //            dr["Piva"] = "";
                    //          }

                    //          this.dts.TargetSC00G100.Rows.Add(dr);
                    //          tbiTarget.SaveDataTable();
                    //          tbiTarget.ConfermaDataset();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //          Console.WriteLine(ex.Message);

                    //        }

                    //        //controllo nella tabella TARGET se esiste o meno il record
                    //        //Faccio update o insert
                    //      }
                    //      else
                    //      {
                    //        //se cfor e' alfanumerico e non sto leggendo l'intestazione della tabella
                    //        //invio la mail di errore
                    //        if (values[1] != "C.For.")
                    //          tsk.InviaMail(false, this.msgmail_istituto_processo + "anomalia singolo record " + line);
                    //      }
                    //    }
                    //  }

                    //  //tbiTarget.SaveDataTable();
                    //  //tbiTarget.ConfermaDataset();
                    //  //Log_Errori.ScriviLOGPROCEDURE("CheckFile fine lettura file ");
                    //  leggi.Close();

                    //  // file esistente e nessun dato caricato
                    //  if (this.dts.TargetSC00G100.Rows.Count == 0)
                    //  {
                    //    tsk.AggiornaStato("3", this.msgmail_istituto_processo + " File trovato, ma nessun dato caricato");
                    //  }
                    //  else
                    //  {
                    //    //Log_Errori.ScriviLOGPROCEDURE("CheckFile aggiornaImportFatture ");

                    //    this.AggiornaImportFatture();
                    //    tsk.AggiornaStato("1", this.msgmail_istituto_processo);
                    //  }
                    //}


                    var parser = new ParserFatture();
                    parser.Esegui(Convert.ToInt32(this.codice_istituto), tsk.path_source, tsk.file_source);
                    parser.check_RdA(Convert.ToInt32(this.codice_istituto), tsk.mail_success, tsk.mail_failure, tsk.mail_mittente);
                    parser.Dispose();


                    tsk.AggiornaStato("1");


        }
        else
        {
                    
          //Log_Errori.ScriviLOGPROCEDURE("CheckFile controlla cutoff");
          //controllo se ha suoeratoil cutoff
          DateTime orario = this.dts.TaskService[0].Orario_Check_File;
          Int16 min = Convert.ToInt16(this.dts.TaskService[0]["Cutoff"].ToString());
          DateTime cutoff = orario.AddMinutes(min);
          if (DateTime.Now > cutoff)
            tsk.AggiornaStato("2",
              this.msgmail_istituto_processo + " " + " File non trovato in CUT-OFF"); //superato            
          else
          {
            //Log_Errori.ScriviLOGPROCEDURE("CheckFile riprogramma quartz");

            tsk.AggiornaStato("0");
            this.ProgrammaQuartz();
            //NON superato
          }
        }

        this.dbInfo.TerminaSessione();
      }
      catch (Exception ex)
      {
        if (this.dbInfo.connessione.State == ConnectionState.Open)
          this.dbInfo.TerminaSessione();

        tsk.AggiornaStato("3", "Import Fatture: " + ex.Message);
        Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "Import Fatture: " + ex.Message);
        PollingExceptionHandler.Esegui(ex);
      }

      //Log_Errori.ScriviLOGPROCEDURE("CheckFile termina" );
    }



    private void AggiornaImportFatture()
    {
      //Log_Errori.ScriviLOGPROCEDURE("AggiornaImportFatture ");
      try
      {
        //controllo che in caso di timeout si sia chiusa la connessione
        if (this.dbInfo.connessione.State == ConnectionState.Closed)
          this.dbInfo.IniziaSessione();


      }
      catch (Exception ex)
      {
        //Console.WriteLine(ex.Message);
        Log_Errori.ScriviLOGPROCEDURE(this.codice_istituto, this.IdProcesso, "AggiornaImportFatture errore " + ex.Message);
        tsk.InviaMail(false, "private void AggiornaImportFatture() " + ex.Message);
        PollingExceptionHandler.Esegui(ex);
      }
    }

    private void ProgrammaQuartz()
    {
      //se e' una chiamata singola, non deve riprogrammare il job
      if (this.OneTime)
        return;

      //Log_Errori.ScriviLOGPROCEDURE("ProgrammaQuartz ");

      ImportFatture_Scheduler sched = new ImportFatture_Scheduler();
      sched.Polling_Interval(tsk.polling_interval, this.codice_istituto, this.IdProcesso);
    }


  }

    public class ImportFatture_Scheduler
    {
        public string messaggio_errore = "";
        private readonly IFornitoriSchedulingGateway schedulingGateway;

        public ImportFatture_Scheduler() : this(new ImportFattureQuartzGateway()) { }

        public ImportFatture_Scheduler(IFornitoriSchedulingGateway schedulingGateway)
        {
            this.schedulingGateway = schedulingGateway ?? throw new ArgumentNullException(nameof(schedulingGateway));
        }

        public bool FirstStart(string codist, string idprocesso)
        {

            return this.FirstStart(codist, idprocesso, false);
        }

        public bool FirstStart(string codist, string idprocesso, bool overrideTask)
        {

            //Log_Errori.ScriviLOGPROCEDURE("ImportFatture_Scheduler.FirstStart ");

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
                var orario_start = "30 8";
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
                else
                {
                    dbInfo.TerminaSessione();
                    esito = false;
                    //codice istituto+idprocesso non trovato
                    throw new PollingException(PollingExceptionType.Task_non_trovato, "");

                }

                dbInfo.TerminaSessione();

                //Log_Errori.ScriviLOGPROCEDURE("FirstStart withCronSchedule " + withCronSchedule);

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
