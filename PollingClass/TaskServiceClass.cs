using System;
using Database;
using Database.TableInterface;
using Database.Base;
using System.Collections;
using System.Data;



/// <summary>
/// Descrizione di riepilogo per TaskServiceClass
/// </summary>
/// 

namespace PollingClass
{
    public class TaskServiceClass
    {

        private ArrayList array;
        private FiltroCampi fc;
        private TableAdapterTaskService tbiTaskService;
        private TableAdapterLogTaskService tbiLogTaskService;
        private dtsTabelle dts = new dtsTabelle();
        private DatabaseInfo dbInfo;

        //private static string codice_istituto = "";
        //private static string codice_guid = "";
        public int polling_interval = 10; //tempo default
        public string mail_failure = "";
        public string mail_success = "";
        public string archivia_path = "";
        public string path_source = "";
        public string file_source = "";
        public bool LasciaInSource = false;
        public string mail_mittente = "";
        private string IdProcesso = "";

        public TaskServiceClass()
        {
        }
        public dtsTabelle LeggiParametri(string codist, string idprocesso)
        {
            //Log_Errori.ScriviLOGPROCEDURE("TaskServiceClass.LeggiParametri " + "nessun tasktrovato " + codist + "-" + idprocesso);
            this.IdProcesso = idprocesso;
            array = new ArrayList();
            int righe = 0;

            dbInfo = new DatabaseInfo();
            dbInfo.IniziaSessione();

            tbiTaskService = new TableAdapterTaskService(dbInfo, dts);
            tbiLogTaskService = new TableAdapterLogTaskService(dbInfo, dts);

            fc = new FiltroCampi();
            fc.Campo = "Istituto";
            fc.Valore = codist;
            fc.Tipo = "N";
            array.Add(fc);

            fc = new FiltroCampi();
            fc.Campo = "IdProcesso";
            fc.Valore = this.IdProcesso;

            array.Add(fc);

            tbiTaskService = new TableAdapterTaskService(dbInfo, dts);
            righe = tbiTaskService.Load(array);
            if (righe > 0)
            {
                polling_interval = Convert.ToInt16(dts.TaskService.Rows[0]["Polling_Intervall"].ToString());
                mail_failure = dts.TaskService.Rows[0]["Mail_Failure"].ToString();
                mail_success = dts.TaskService.Rows[0]["Mail_success"].ToString();
                archivia_path = dts.TaskService.Rows[0]["Path_Archiviazione"].ToString();
                path_source = dts.TaskService[0].Path_Source;
                file_source = dts.TaskService[0].File_Source;
                LasciaInSource = dts.TaskService[0].LasciaInSource;
                mail_mittente = dts.TaskService.Rows[0]["Mail_Mittente"].ToString();
                this.IdProcesso = dts.TaskService.Rows[0]["IdProcesso"].ToString();
            }
            else
            {
                throw new PollingException(PollingExceptionType.Task_non_trovato, "task non trovato");
            }

            array = new ArrayList();

            fc = new FiltroCampi();
            fc.Campo = "Istituto";
            fc.Valore = codist;
            fc.Tipo = "N";
            array.Add(fc);

            fc = new FiltroCampi();
            fc.Campo = "IdProcesso";
            fc.Valore = this.IdProcesso;
            array.Add(fc);

            fc = new FiltroCampi();
            fc.Campo = "Ora_fine";
            fc.Tipo = "N";
            fc.SegnoOp = "";
            fc.Valore = "is null";
            array.Add(fc);

            fc = new FiltroCampi();
            fc.Campo = "Data";
            fc.Tipo = "D";
            fc.Valore = DateTime.Now.ToShortDateString();
            array.Add(fc);

            tbiLogTaskService = new TableAdapterLogTaskService(dbInfo, dts);
            righe = tbiLogTaskService.Load(array);

            //se intervallo e' = 0, significa che e' in stato disattivato
            if (righe == 0)// && this.polling_interval>0
            {
                //Log_Errori.ScriviLOGPROCEDURE("TaskServiceClass.LeggiParametri " + "inserisco riga LogTaskService");

                //inserimento Lettura giornaliera per codice istituto
                DataRow dr = dts.LogTaskService.NewLogTaskServiceRow();
                dr["Istituto"] = codist;
                dr["Data"] = DateTime.Now.ToShortDateString();
                dr["IdProcesso"] = this.IdProcesso;
                dr["Codice_stato"] = 0;
                dr["Ora_Inizio"] = DateTime.Now;
                dts.LogTaskService.Rows.Add(dr);
                tbiLogTaskService.SaveDataTable();
                tbiLogTaskService.ConfermaDataset();
            }

            dbInfo.TerminaSessione();
            return dts;
        }

        public void InviaMail(bool success, string messaggio_aggiuntivo)
        {
            //Log_Errori.ScriviLOGPROCEDURE("TaskServiceClass.InviaMail " + success.ToString());

            MailSede mail = new MailSede();
            if (success)
                mail.Invia(mail_success, mail_mittente, "Result OK", messaggio_aggiuntivo);
            else
                mail.Invia(mail_failure, mail_mittente, "Result FAIL", messaggio_aggiuntivo);
        }

        public void ArchiviaFile()
        {
            try
            {
                if (archivia_path == "")
                    return;

                //sposta il file nella cartella di destinazione
                //processo_ora_minuti_nomefile
                string archivia = DateTime.Now.Year.ToString() + "_" + string.Format("{0:00}", DateTime.Now.Month) + "_" + string.Format("{0:00}", DateTime.Now.Day) + "_" +
                        string.Format("{0:00}", DateTime.Now.Hour) + "_" + string.Format("{0:00}", DateTime.Now.Minute);
                if (this.LasciaInSource) //copio il file nella cartella di destinazione
                {
                    System.IO.File.Copy(ConfiguredFilePaths.Combine(path_source, file_source),
                        ConfiguredFilePaths.Combine(archivia_path, this.IdProcesso + "_" + archivia + "_" + file_source)
                        );
                }
                else //sposto il file cartella di destinazione
                {
                    System.IO.File.Move(ConfiguredFilePaths.Combine(path_source, file_source),
                      ConfiguredFilePaths.Combine(archivia_path, this.IdProcesso + "_" + archivia + "_" + file_source)
                      );
                }
            }
            catch (Exception ex)
            {
                this.InviaMail(false, "Errore archiviazione file: "+ex.Message);
            }
        }


    #region AGGIORNA STATO LOG
    public  void AggiornaStato(string codsta)
    {
        AggiornaStato(codsta, "");

    }

    public  void AggiornaStato(string codsta, string message)
    {
        try
        {
            //Log_Errori.ScriviLOGPROCEDURE("TaskServiceClass.AggiornaStato " );

            //controllo che in caso di timeout si fosse chiusa la connessione
            if (dbInfo.connessione.State == ConnectionState.Closed)
                dbInfo.IniziaSessione();


            dts.LogTaskService[0].BeginEdit();
            dts.LogTaskService[0]["Codice_stato"] = codsta;

            switch (codsta)
            {
                case "0": //in Esecuzione                      
                    //viene lanciato dalle classi chiamanti
                    //this.ProgrammaQuartz();
                    break;

                case "1": //file trovato 
                    dts.LogTaskService[0]["Ora_fine"] = DateTime.Now.ToShortTimeString();
                    //programmare Quartz al giorno successivo
                    //this.ProgrammaQuartz();
                    this.InviaMail(true, message);
                    ArchiviaFile();
                    break;

                case "2":  //file non trovato fuori cutoff          
                    dts.LogTaskService[0]["Ora_fine"] = DateTime.Now.ToShortTimeString();
                    this.InviaMail(false, message);
                    break;

                case "3":  //errore                
                    dts.LogTaskService[0]["Ora_fine"] = DateTime.Now.ToShortTimeString();
                    this.InviaMail(false, message);
                    break;

                case "4": //in elaborazione file
                    dts.LogTaskService[0]["Ora_Inizio"] = DateTime.Now.ToShortTimeString();
                    break;

            }
            dts.LogTaskService[0]["Messaggio_Return"] = message.Length > 100 ? message.Substring(0, 100) : message;
            dts.LogTaskService[0].EndEdit();

            //aggiorna database
            tbiLogTaskService.SaveDataTable();
            tbiLogTaskService.ConfermaDataset();

            //Log_Errori.ScriviLOGPROCEDURE("TaskServiceClass.AggiornaStato " + codsta);
        }
        catch (Exception ex)
        {
            this.InviaMail(false, "private void AggiornaStato(string codsta,string message), Codice Stato="+codsta  +" "+ ex.Message);
        }


    }

    #endregion
}
}