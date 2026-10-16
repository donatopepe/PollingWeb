using System;
using Database.Configuration;
using System.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Database.Base
{
    /// <summary>
    /// Descrizione di riepilogo per Connessioni.
    /// </summary>
    /// 


    public class DatabaseInfo
    {


        #region ATTRIBUTI STATICI (CLASS MEMBERS)


        //server dell'istanza di sql server
        public static string data_source;

        //username e password dell'account di accesso a sql server
        public static string user_id;
        public static string password;

        //nome del database
        public static string initial_catalog;

        public static string timeOut;

        //stringa di connessione
        //Esempio:
        //"data source=DC01;user id=sa;$1=[REDACTED];initial catalog=Retail;integrated security=false;persist security info=False;packet size=4096" />    
        private static IAppConfig appConfig = new AppConfig();
        private static string stringaConnessione;
        private static string stringaConnessioneEproc;
        public static IAppConfig AppConfigProvider
        {
            get { return appConfig; }
            set { appConfig = value ?? throw new ArgumentNullException(nameof(value)); }
        }

        public static void Configure(IAppConfig configuration)
        {
            AppConfigProvider = configuration;
        }

        public static string StringaConnessione
        {
            //get	{return stringaConnessione;}
            get
            {
                string timeout = (timeOut != null && timeOut != "") ? ";Connect Timeout=" + timeOut : "";

                //stringaConnessione =
                //    "data source=(local)\\Sqlexpress;" +
                //    "user id=sa;" +
                //    "$1=[REDACTED];" +
                //    //"initial catalog=" + initial_catalog + ";" +
                //    "initial catalog=Polling;" +
                //    "integrated security=false;persist security info=False;packet size=4096" +
                //    timeout;

                stringaConnessione = appConfig.GetConnectionString("PollingConnectionString");
                if (string.IsNullOrWhiteSpace(stringaConnessione))
                    throw new ConfigurationErrorsException("Connection string 'PollingConnectionString' is not configured.");

                //versione local
                //stringaConnessione = "data source=.\\SQLEXPRESS;initial catalog=TEST_01;integrated security=true;persist security info=False;packet size=4096;Connect Timeout=100";

                return stringaConnessione;
            }

        }

        public static string StringaConnessioneEproc
        {           
            get
            {
                string timeout = (timeOut != null && timeOut != "") ? ";Connect Timeout=" + timeOut : "";

                stringaConnessioneEproc = appConfig.GetConnectionString("PollingEprocurement");
                if (string.IsNullOrWhiteSpace(stringaConnessioneEproc))
                    throw new ConfigurationErrorsException("Connection string 'PollingEprocurement' is not configured.");

                 return stringaConnessioneEproc;
            }
        }






        #endregion




        #region ATTRIBUTI NON STATICI (INSTANCE MEMBERS)


        //CONNESSIONE 
        public SqlConnection connessione;


        //TRANSAZIONE
        public SqlTransaction transazione;




        #endregion





        #region COSTRUTTORI


        public DatabaseInfo()
        {
            this.connessione = new SqlConnection(DatabaseInfo.StringaConnessione);

        }
        public DatabaseInfo(bool eprocurement)
        {
            this.connessione = new SqlConnection(DatabaseInfo.StringaConnessioneEproc);

        }



        #endregion





        #region METODI PUBBLICI



        //SESSIONE DATABASE 


        public void IniziaSessione()
        {
            try
            {
                this.connessione.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void TerminaSessione()
        {
            this.connessione.Close();
        }


        //GESTIONE TRANSAZIONI


        public void IniziaTransazione()
        {
            if (this.connessione.State != ConnectionState.Open)
            {
                this.connessione.Open();
            }

            this.transazione = this.connessione.BeginTransaction();
        }




        public void AnnullaTransazione()
        {
            if (this.transazione != null && this.transazione.Connection != null)
                this.transazione.Rollback();
        }




        public void ConfermaTransazione()
        {
            this.transazione.Commit();
        }



        #endregion



    }



}
