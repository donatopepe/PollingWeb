using Database;
using Database.Base;
using Database.TableInterface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollingClass
{
    class ParserFatture : IDisposable
    {
        DatabaseInfo dbInfoPolling;
        DatabaseInfo dbInfoeProcurement;
        public void Dispose()
        {
            if (dbInfoPolling != null) dbInfoPolling.TerminaSessione();
            if (dbInfoeProcurement != null) dbInfoeProcurement.TerminaSessione();
        }
        public bool IsNumeric(string s)
        {
            double output;
            return double.TryParse(s, out output);
        }
        public string Trim(string str)
        {
            if (String.IsNullOrWhiteSpace(str)) return string.Empty;
            return str.Trim();
        }

        public string Mid(string str, int index, int length)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            var lungh = str.Length;
            var min = Math.Min(lungh, index - 1);
            str = str.Remove(0, min);
            return Left(str, length);
        }
        public string Replace(string str, object oldStr, object newStr)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            var oldS = Convert.ToString(oldStr);
            var newS = Convert.ToString(newStr);
            return str.Replace(oldS, newS);
        }
        public string Left(string str, int index)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            var lungh = Math.Min(str.Length, Math.Min(str.Length, index));
            var result = str.Substring(0, lungh);
            return result;
        }
        public string Right(string str, int index)
        {
            if (String.IsNullOrWhiteSpace(str)) return str;
            var result = str.Substring(str.Length - Math.Min(str.Length, index));
            return result;
        }

        public int getInt(string value)
        {
            int numint;
            var style = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowThousands;
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("it-IT");
            if (Int32.TryParse(value, style, culture, out numint))
                return numint;
            else
                return 0;

        }
        public double getdouble(string value)
        {
            double numdouble = 0;
            var style = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowThousands;
            var culture = System.Globalization.CultureInfo.CreateSpecificCulture("it-IT");
            if (Double.TryParse(value, style, culture, out numdouble))
                return numdouble;
            else
                return 0;

        }


        public string ew_EscapeJs(string str)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            str = str.Replace("'", "`");
            str = str.Replace("\"", "`");
            return str;

        }

        public string SQLType(object pvValue)
        {
            if (pvValue == null) return "NULL";
            Type type = pvValue.GetType();
            var sTemp = Convert.ToString(pvValue);
            if (type == typeof(int))
            {
                sTemp = Convert.ToInt32(pvValue).ToString();
            }
            if (type == typeof(double))
            {
                sTemp = Convert.ToDouble(pvValue).ToString("F");
            }
            if (type == typeof(DateTime))
            {
                sTemp = "'" + Convert.ToDateTime(pvValue).ToString("dd/MM/yyyy") + "'";
                return sTemp;
            }

            if (String.IsNullOrWhiteSpace(sTemp)) return "NULL";
            sTemp = Replace(sTemp, ",", ".");
            sTemp = Replace(sTemp, "'", "''");
            sTemp = Replace(sTemp, "-", " ");

            sTemp = AntiSqlInject.Instance.GetSafetySql(sTemp); //aggiunto per evitare sqlinjection
            if (type == typeof(string))
            {
                // String
                sTemp = "'" + sTemp + "'";
            }
            return sTemp;
        }
        private List<string> FormatoDate = new List<string>()
        {
            "dd.MM.yy",
            "dd.MM.yyyy",
            "ddMMyy",
            "ddMMyyyy",
            "dd/MM/yyyy",
            "dd/MM/yy",
            "dd/MM/yyyy H.mm.ss",
            "dd/MM/yyyy HH.mm.ss",
            "yyyyMMdd",
        };

        public ParserFatture()
        {
            dbInfoPolling = new DatabaseInfo();
            dbInfoPolling.IniziaSessione();
            dbInfoeProcurement = new DatabaseInfo(true);
            dbInfoeProcurement.IniziaSessione();
        }

        public DateTime? TextToDate(string value, bool viewerror = true)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            DateTime result = DateTime.MinValue;
            var dataOK = false;
            foreach (var item in FormatoDate)
            {
                dataOK = DateTime.TryParseExact(value, item, CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
                if (dataOK) break;
            }

            if (!dataOK)
            {
                dataOK = DateTime.TryParse(value, CultureInfo.GetCultureInfo("it-IT"), DateTimeStyles.None, out result);
                if (!dataOK)
                {
                    dataOK = DateTime.TryParse(value, CultureInfo.GetCultureInfo("en-EN"), DateTimeStyles.None, out result);
                    if (!dataOK)
                    {
                        return null;
                    }
                }
            }
            return result;
        }
        public void Esegui(int CodIst, string PATH_FOLDER, string oNomeFile)
        {


            // La connessione al database eProcurement arriva dalla configurazione
            // (connection string "PollingEprocurement"). Non inserire mai
            // credenziali o host reali nel codice.

            var VAI_A_CAPO = (char)(10) + (char)(13);
            var VAI_PAGINA_SUCCESSIVA = (char)(175) + VAI_A_CAPO;
            var END_TAG = (char)(178) + (char)(45) + "END" + (char)(45);
            var sFile = PATH_FOLDER + @"\" + oNomeFile;

            if (File.Exists(sFile))
            {

                var sTab_CaptureDate = File.GetLastWriteTime(sFile);

                var Tab_CaptureDate = sTab_CaptureDate.ToShortDateString();
                using (var f = new StreamReader(sFile))
                {
                    // ***************************************************************************************************************************************
                    // *					BEGIN  -  ALGORITMO DI PARSERIZZAZIONE				*
                    // ***************************************************************************************************************************************

                    // ***********  usato per sapere se è stato incontrato almeno un tabulato per la INSERT   ***************	
                    var Trovato = false;   // *****  quando è false permette di saltare tutte le righe precedenti la prima occorrenza di ##EOS#   ********
                    var Tab_Ist = System.Convert.ToInt32(CodIst);
                    var Tab_Name = "";
                    var Tab_Prog = "";
                    var Tab_Data = "";
                    var nordine = 0d;
                    var nrigap = 0;
                    var nriga = 0;
                    var materiale = "";
                    var testo_breve = "";
                    var gr_merci = "";
                    var qta = 0d;
                    var umo = "";
                    var prz_netto = 0d;

                    var salvaOR = false;
                    var salvaDF = false;
                    while (!f.EndOfStream)
                    {

                        var r = f.ReadLine();
                        var riga = Replace(r, (char)(0), (char)(32));

                        var strTop = Left(riga, 6);
                        var strEnd = Left(riga, 6);

                        if ((strTop == "470901"))
                        {

                            // msgBox "linea: " + linea

                            Tab_Name = Trim(Mid(riga, 7, 8));
                            //Tab_Prog = Trim(Mid(riga,24,6))

                            var sTab_Data = Trim(Mid(riga, 27, 6));
                            var aaaa = "20" + Left(sTab_Data, 2);
                            var mm = Mid(sTab_Data, 3, 2);
                            var gg = Right(sTab_Data, 2);

                            Tab_Data = gg + "/" + mm + "/" + aaaa;

                            Trovato = true;   // ***  ho incontrato la prima sezione e dal prossimo ciclo posso leggere le righe nel textbody  *****
                            continue;
                        }
                        else
                        if ((strEnd == END_TAG))
                        {
                            Trovato = false;// *** salta le righe tra END_TAG ed un possibile nuovo START_TAG ##EOS# successivo
                            continue;
                        }
                            

                        if (Trovato)
                        {



                            // *******dati identificativi ordine*******
                            var tpo = "";
                            tpo = Trim(Mid(riga, 14, 4));

                            if ((tpo == "NB" | tpo == "NB01" | tpo == "NB46" | tpo == "NBC" | tpo == "ZA"))
                            {


                                nordine = getdouble(Trim(Mid(riga, 3, 11)));


                                var sSQLO = "SELECT [NOrdine]" +
                                    " FROM [eProcurement].[dbo].[Tab_SC00S100_1Ord]" +
                                    " WHERE  [NOrdine]=" + SQLType(nordine);

                                //SqlCommand command = new SqlCommand(sSQLO, dbInfoeProcurement.connessione);
                                //SqlDataReader dataReader = command.ExecuteReader();
                                //var sSQLO_OK = dataReader.Read();
                                //dataReader.Close();
                                //var sSQLO_data =Aggset.EseguiQuery("eProcurement", sSQLO, dbInfoeProcurement).Tables[0].Rows.Count;
                                //var pippo = sSQLO_data.Tables[0].Rows.Count;
                                if (Aggset.EseguiQuery("eProcurement", sSQLO, dbInfoeProcurement).Tables[0].Rows.Count <= 0)
                                {
                                    var tipo = Trim(Mid(riga, 14, 4));
                                    var codfor = getInt(Trim(Mid(riga, 19, 10)));
                                    var ragsoc = Trim(Mid(riga, 30, 36));
                                    var gra = Trim(Mid(riga, 67, 3));
                                    var data_ordacq = TextToDate(Trim(Mid(riga, 71, 10)));

                                    //'*******************************************
                                    //'*   INSERIMENTO DELLA RIGA SUL DB         *
                                    //'*******************************************

                                    var sSQL = "INSERT INTO [Tab_SC00S100_1Ord] " +
                                               "([Tab_Name]" +
                                               ",[Tab_Prog]" +
                                               ",[Istituto]" +
                                               ",[Data_Rif]" +
                                               ",[NOrdine]" +
                                               ",[Tipo]" +
                                               ",[CodFor]" +
                                               ",[RagSoc]" +
                                               ",[GrA]" +
                                               ",[Data_OrdAcq])" +
                                         " VALUES " +
                                                "(" +
                                                SQLType(Tab_Name) + "," +
                                                "" + SQLType(Tab_Prog) + "," +
                                                "" + SQLType(Tab_Ist) + "," +
                                                "" + SQLType(Tab_Data) + "," +
                                                "" + SQLType(nordine) + "," +
                                                "" + SQLType(tipo) + "," +
                                                "" + SQLType(codfor) + "," +
                                                "" + SQLType(ew_EscapeJs(ragsoc)) + "," +
                                                "" + SQLType(gra) + "," +
                                                "" + SQLType(data_ordacq) + ")";



                                    // *******************************************
                                    // *   INSERIMENTO DELLA RIGA SUL DB         *
                                    // *******************************************

                                    if (Aggset.Eseguicomando(sSQL, dbInfoeProcurement) <= 0)
                                        throw (new Exception("Errore: " + sSQL)); // ******   esco dal ciclo  *******

                                }
                                continue;
                            }


                            // *******dati riga ordine in Tab_SC00S100_1Ord_R*******
                            nrigap = getInt(Trim(Mid(riga, 3, 7)));
                            if ((nrigap > 0) && (nordine > 0))
                            {
                                salvaOR = false;
                                salvaDF = false;

                                //'**** righe relative alla stessa fattura dello stesso ordine che devono essere memorizzate
                                var sSQLOR = "SELECT [NOrdine], [Perc_DaFatt]" +
                                " FROM [eProcurement].[dbo].[Tab_SC00S100_1Ord_R]" +
                                " WHERE  [NOrdine]=" + SQLType(nordine) + " AND NRiga=" + SQLType(nrigap);

                                if (Aggset.EseguiQuery("eProcurement", sSQLOR, dbInfoeProcurement).Tables[0].Rows.Count <= 0)
                                {
                                    nriga = getInt(Trim(Mid(riga, 3, 7)));
                                    materiale = (Trim(Mid(riga, 11, 12)));
                                    testo_breve = Trim(Mid(riga, 30, 40));
                                    gr_merci = Trim(Mid(riga, 71, 10));
                                }
                                else
                                {
                                    nriga = 0;
                                }
                                continue;
                            }

                            var c = Trim(Mid(riga, 9, 1));
                            if ((nriga > 0) && (nordine > 0) && (c == "Y" || c == "A"))
                            {
                                // **** righe relative alla stessa fattura dello stesso ordine che devono essere memorizzate

                                qta = getdouble(Trim(Mid(riga, 35, 10)));
                                umo = Trim(Mid(riga, 47, 3));
                                prz_netto = getdouble(Trim(Mid(riga, 51, 15)));

                                salvaOR = true;
                                salvaDF = false;
                                continue;
                            }

                            if ((salvaOR == true) && (nriga > 0) && (nordine > 0))
                            {
                                //'*******************************************
                                //'*   INSERIMENTO DELLA RIGA SUL DB         *
                                //'*******************************************

                                var sSQLOR = "INSERT INTO [Tab_SC00S100_1Ord_R] " +
                                       "([NOrdine]" +
                                       ",[NRiga]" +
                                       ",[Materiale]" +
                                       ",[Testo_Breve]" +
                                       ",[Gr_Merci]" +
                                       ",[Qta]" +
                                       ",[UMO]" +
                                       ",[Prz_Netto])" +
                                 " VALUES " +
                                        "(" + SQLType(nordine) + "," +
                                        "" + SQLType(nriga) + "," +
                                        "" + SQLType(materiale) + "," +
                                        "" + SQLType(ew_EscapeJs(testo_breve)) + "," +
                                        "" + SQLType(gr_merci) + "," +
                                        "" + SQLType(qta) + "," +
                                        "" + SQLType(umo) + "," +
                                        "" + SQLType(prz_netto) + ")";


                                // *******************************************
                                // *   INSERIMENTO DELLA RIGA SUL DB         *
                                // *******************************************

                                if (Aggset.Eseguicomando(sSQLOR, dbInfoeProcurement) <= 0)
                                    throw (new Exception("Errore: " + sSQLOR)); // ******   esco dal ciclo  *******

                                salvaOR = false;
                                salvaDF = true;
                                continue;
                            } // salva righe ordine = true

                            // se % da fatturare diversa da 0 inserisco i dati dell'importo ancora da fatturare
                            var dafatt = Trim(Mid(riga, 8, 15));
                            // msgbox dafatt + "-" + salvaDF
                            if ((dafatt == "Ancora da fatt.") && (salvaDF == true) && (nriga > 0) && (nordine > 0))
                            {
                                // **** righe relative alla stessa fattura dello stesso ordine che devono essere memorizzate

                                var qta_dafatt = getdouble(Trim(Mid(riga, 35, 10)));
                                var umo_dafatt = Trim(Mid(riga, 47, 3));
                                var prz_netto_dafatt = getdouble(Trim(Mid(riga, 51, 15)));
                                var perc_dafatt = Trim(Mid(riga, 73, 6));

                                //'*******************************************
                                //'*   INSERIMENTO DELLA RIGA SUL DB         *
                                //'*******************************************

                                var sSQLDF = "UPDATE [Tab_SC00S100_1Ord_R] " +
                                       " SET [Qta_DaFatt] =" + SQLType(qta_dafatt) + "" +
                                             " ,[UMO_DaFatt] =" + SQLType(umo_dafatt) + "" +
                                             " ,[Prz_Netto_DaFatt] =" + SQLType(prz_netto_dafatt) + "" +
                                             " ,[Perc_DaFatt] =" + SQLType(perc_dafatt) + "" +
                                       " WHERE  [NOrdine]=" + SQLType(nordine) + " AND NRiga=" + SQLType(nriga);


                                // *******************************************
                                // *   INSERIMENTO DELLA RIGA SUL DB         *
                                // *******************************************

                                if (Aggset.Eseguicomando(sSQLDF, dbInfoeProcurement) <= 0)
                                {
                                    //throw (new Exception("Errore: " + sSQLDF)); // ******   esco dal ciclo  *******   // commentato Donato perchè capita che la riga è assente
                                }
                                salvaDF = false;
                                continue;
                            } // salva righe salvaDF = true						


                            // *******dati fattura in Tab_SC00S100_2Fatt*******
                            var tpf = Trim(Mid(riga, 3, 4));
                            int nfatt = getInt(Trim(Mid(riga, 8, 10)));

                            if ((tpf == "CF" || tpf == "NB-L") && (nfatt > 0) && (nordine > 0))
                            {
                                var sSQLFattura = "SELECT [NOrdine]" +
                                    " FROM [eProcurement].[dbo].[Tab_SC00S100_2Fatt]" +
                                    " WHERE  [NOrdine]=" + SQLType(nordine) + " AND [NFatt]=" + SQLType(nfatt);

                                if (Aggset.EseguiQuery("eProcurement", sSQLFattura, dbInfoeProcurement).Tables[0].Rows.Count <= 0)
                                {
                                    var data_fatt = TextToDate(Trim(Mid(riga, 28, 8)));

                                    //'*******************************************
                                    //'*   INSERIMENTO DELLA RIGA SUL DB         *
                                    //'*******************************************

                                    var sSQLF = "INSERT INTO [Tab_SC00S100_2Fatt] " +
                                               "([NOrdine]" +
                                               ",[NFatt]" +
                                               ",[Data_Fatt])" +
                                         " VALUES " +
                                                "(" + SQLType(nordine) + "," +
                                                "" + SQLType(nfatt) + "," +
                                                "" + SQLType(data_fatt) + ")";

                                    // *******************************************
                                    // *   INSERIMENTO DELLA RIGA SUL DB         *
                                    // *******************************************

                                    if (Aggset.Eseguicomando(sSQLF, dbInfoeProcurement) <= 0)
                                        throw (new Exception("Errore: " + sSQLF)); // ******   esco dal ciclo  *******
                                }


                                // **** righe relative alle posizioni della fattura *****	
                                var npos = getInt(Trim(Mid(riga, 19, 4)));


                                var sSQLFattR = "SELECT [NOrdine]" +
                                    " FROM [eProcurement].[dbo].[Tab_SC00S100_2Fatt_R]" +
                                    " WHERE  [NOrdine]=" + SQLType(nordine) + " AND [NFatt]=" + SQLType(nfatt) + " AND [NPos]=" + SQLType(npos);

                                if (Aggset.EseguiQuery("eProcurement", sSQLFattR, dbInfoeProcurement).Tables[0].Rows.Count <= 0)
                                {
                                    var qta_tot = getdouble(Trim(Mid(riga, 46, 8)));
                                    umo = Trim(Mid(riga, 55, 3));
                                    var prz_tot = getdouble(Trim(Mid(riga, 59, 17)));

                                    //'*******************************************
                                    //'*   INSERIMENTO DELLA RIGA SUL DB         *
                                    //'*******************************************

                                    var sSQLFR = "INSERT INTO [Tab_SC00S100_2Fatt_R] " +
                                               "([NOrdine]" +
                                               ",[NFatt]" +
                                               ",[NPos]" +
                                               ",[Qta_Tot]" +
                                               ",[UMO]" +
                                               ",[Prz_Tot])" +
                                         " VALUES " +
                                                "(" + SQLType(nordine) + "," +
                                                "" + SQLType(nfatt) + "," +
                                                "" + SQLType(npos) + "," +
                                                "" + SQLType(qta_tot) + "," +
                                                "" + SQLType(umo) + "," +
                                                "" + SQLType(prz_tot) + ")";


                                    // *******************************************
                                    // *   INSERIMENTO DELLA RIGA SUL DB         *
                                    // *******************************************

                                    if (Aggset.Eseguicomando(sSQLFR, dbInfoeProcurement) <= 0)
                                        throw (new Exception("Errore: " + sSQLFR)); // ******   esco dal ciclo  *******
                                }

                            }
                        }

                    }
                    // ***************************************************************************************************************************************
                    // *					END  -  ALGORITMO DI PARSERIZZAZIONE				*
                    // ***************************************************************************************************************************************
                }
  
            }
        }


        public void check_RdA(int CodIst, string mail_success, string mail_failure, string mail_mittente)
        {

            //VERIFICO GLI ORDINI DELLE RDA
            var sSQLRdA = "SELECT [GUID], [NOrdSAP], [Progressivo]" +
            " FROM [RdA]" +
            " WHERE ([Verificato] IS NULL OR [Verificato]=0) AND [Status]=20 AND Istituto=" + CodIst;

            var rsRdAtables = Aggset.EseguiQuery("RdA", sSQLRdA, dbInfoeProcurement);

            foreach (DataRow rsRdA in rsRdAtables.Tables[0].Rows)
            {
                var GUID = rsRdA["GUID"];
                var NOrdSAP = rsRdA["NOrdSAP"];
                var Progressivo = rsRdA["Progressivo"];

                //VERIFICO SE L'ORDINE E' PRESENTE NEL FILE DEGLI ORDINI
                var sSQLO = "SELECT [NOrdine] " +
                " FROM [Tab_SC00S100_1Ord] " +
                " WHERE NOrdine = " + NOrdSAP;

                var sSQLOtable = Aggset.EseguiQuery("Tab_SC00S100_1Ord", sSQLO, dbInfoeProcurement);

                if (sSQLOtable.Tables[0].Rows.Count <= 0)
                {
                    //imposto il campo [Verificato] della RdA a 0
                    var sSQLDR = "UPDATE [RdA] " +
                           " SET [Verificato] = 0" +
                           " WHERE  [GUID]='" + GUID + "'";

                    if (Aggset.Eseguicomando(sSQLDR, dbInfoeProcurement) <= 0)
                        throw (new Exception("Errore: " + sSQLDR)); // ******   esco dal ciclo  *******
                }
                else
                {
                    //imposto il campo [Verificato] della RdA a 1
                    var sSQLDF = "UPDATE [RdA] " +
                           " SET [Verificato] =1" +
                           " WHERE  [GUID]='" + GUID + "'";

                    if (Aggset.Eseguicomando(sSQLDF, dbInfoeProcurement) <= 0)
                        throw(new Exception("Errore: "+sSQLDF));// ******   esco dal ciclo  ******
                }

            };
            //VERIFICO GLI ORDINI CHE SONO ANCORA ERRATI PER INVIARE LE MAIL
            sSQLRdA = "SELECT     RdA.GUID, RdA.Progressivo, RdA.NOrdSAP, RdA.Verificato, LogTime_Ciclo.Matricola, DimResource.EMail " +
                "FROM    RdA INNER JOIN " +
                "LogTime_Ciclo ON RdA.GUID = LogTime_Ciclo.Rch_ID AND RdA.Status = LogTime_Ciclo.StatoID INNER JOIN " +
                "DimResource ON LogTime_Ciclo.Matricola = DimResource.Matricola " +
                "WHERE  (RdA.Verificato = 0)  AND Istituto=" + CodIst
                + " AND YEAR(RdA.DataCreazione) = YEAR(GETDATE())"
                + " ORDER BY MATRICOLA";   //

            var sSQLRdAtables = Aggset.EseguiQuery("RdA", sSQLRdA, dbInfoeProcurement);
            var elencoRdA = "";
            string Matricola_prec = null;
            string DestMailn = null;
            foreach (DataRow rsRdA in sSQLRdAtables.Tables[0].Rows)
            {
                var GUID = rsRdA["GUID"];
                string Matricola = Convert.ToString(rsRdA["Matricola"]);
                var NOrdSAP = rsRdA["NOrdSAP"];
                var Progressivo = rsRdA["Progressivo"];

                var stringa = "RCH_ID=" + GUID + "&RCH_IDRDE=&RCH_IDRDA=" + GUID + "&RCH=RDA&RCH_PROGR=" + Progressivo + "&IDPage=2&nRecord=0&Mode=UPDATE&SelCodice=&SelCategoria=&SelCognome=&SelNome=&SelStato=&SelDataCreazione=&SelDestDescr=";

                if (DestMailn!=null && !String.IsNullOrWhiteSpace(elencoRdA) && Matricola_prec != null && (Matricola_prec != Matricola))
                {
                    elencoRdA = Left(elencoRdA, elencoRdA.Length - 1);
                    SendMail(elencoRdA, DestMailn, mail_failure, mail_mittente);
                    elencoRdA = "";
                }

                DestMailn = Convert.ToString(rsRdA["EMail"]);

                elencoRdA = elencoRdA + " <a href=\"" + "http://<INTRANET-HOST>/webapp/redirect.aspx?url=http://<APP-HOST>/getuserinfo/GetUserInfo.asp?UrlRedirect=http://<APP-HOST>/eProcurement/login.asp?" + stringa + "\"" + ">" + Progressivo + "</a>,";

                Matricola_prec = Matricola;

            }
            if (!String.IsNullOrWhiteSpace(elencoRdA))
            {
                elencoRdA = Left(elencoRdA, elencoRdA.Length - 1);
                SendMail(elencoRdA, DestMailn, mail_failure, mail_mittente);
                elencoRdA = "";
            }

        }


        public void SendMail(object elencoRdA, object DestMailn, string mail_CC, string mail_mittente)
        {
            //"dbo.Sp_CdoMail"; // Stored Procedure Name
            //"@From", <mittente-da-configurazione>
            //"@To", DestMailn
            //"@To", <destinatario-da-configurazione>
            //"@Subject", "Notifica Ordine SAP da verificare"

            var sHTML = "<html><head>";
            sHTML = sHTML + "<style type=\"text/css\">";
            sHTML = sHTML + ".testo {";
            sHTML = sHTML + "	font-family: Verdana, Arial, Helvetica, sans-serif;";
            sHTML = sHTML + "	font-size: 10px;";
            sHTML = sHTML + "	line-height: 14px;";
            sHTML = sHTML + "	letter-spacing: normal;}";
            sHTML = sHTML + ".Font8Boldv {";
            sHTML = sHTML + "	FONT-WEIGHT: bold;";
            sHTML = sHTML + "	FONT-SIZE: 10px;";
            sHTML = sHTML + "	COLOR: black;";
            sHTML = sHTML + "	FONT-FAMILY: Verdana, Arial;}";
            sHTML = sHTML + ".Font8Boldr {";
            sHTML = sHTML + "	FONT-WEIGHT: bold;";
            sHTML = sHTML + "	FONT-SIZE: 10px;";
            sHTML = sHTML + "	COLOR: #A52A2A;";
            sHTML = sHTML + "	FONT-FAMILY: Verdana, Arial;}";
            sHTML = sHTML + "</style></head>";
            sHTML = sHTML + "<body>	";
            sHTML = sHTML + "<table border=0 cellspacing=1 cellpadding=0 bgcolor=\"#dbdbdb\" align=center width=\"500\">";
            sHTML = sHTML + "<tr><td><tbody>";
            sHTML = sHTML + "		<table border=0 cellspacing=0 cellpadding=10 width=\"100%\"  bgcolor=white>";
            sHTML = sHTML + "		<tr><td class=\"Font8Boldv\">";
            sHTML = sHTML + "				<TABLE BORDER=0 WIDTH=\"500\" CELLSPACING=0 CELLPADDING=0  align=center > ";
            sHTML = sHTML + "				<tr><td class=\"Font8Boldr\" align=right>ORDINI DA VERIFICARE";
            sHTML = sHTML + "					</td></tr>";
            sHTML = sHTML + "				<tr><td class=testo align=\"justify\"><br><br>";
            sHTML = sHTML + "					Buongiorno!<br><br>";
            sHTML = sHTML + "					<p align=justify>";
            sHTML = sHTML + "					<br><br>";
            sHTML = sHTML + "					E' stato riscontrato un errore nel numero di ordine SAP inserito nella/e RdA ";

            sHTML = sHTML + elencoRdA + ".<br>";

            sHTML = sHTML + @"					E' possibile effettuare la ricerca delle RdA errate dal Menu Cost Management \ RdA Tutte <br>";
            sHTML = sHTML + "					selezionando dalla funzione di ricerca: Ordini errati. ";

            sHTML = sHTML + "					<br>";
            sHTML = sHTML + "					<br>";

            sHTML = sHTML + "					Saluti.";
            sHTML = sHTML + "					</p>";
            sHTML = sHTML + "					</td></tr></table>";
            sHTML = sHTML + " 			</td></tr></table>";
            sHTML = sHTML + "		</tbody></td></tr>";
            sHTML = sHTML + "</table>";
            sHTML = sHTML + "</body></html>";

            MailSede mail = new MailSede();

            //mail.Invia(DestMailn + ";" + mail_CC, mail_mittente, "Notifica Ordine SAP da verificare", sHTML);
            mail.Invia(mail_CC, mail_mittente, "Notifica Ordine SAP da verificare "+ DestMailn, sHTML);
            //mail.Invia(<destinatario-da-configurazione>, mail_mittente, "Notifica Ordine SAP da verificare " + DestMailn, sHTML);
        }



































    }
}