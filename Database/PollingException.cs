using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Descrizione di riepilogo per PollingException
/// </summary>
/// 

namespace Database
{
    public enum PollingExceptionType
    {
        Generico,
        Job_Quartz_esistente,
        Task_non_trovato,
        Task_in_esecuzione,
        Inviomail,
        file_non_trovato,
        file_presente_nella_cartella

    }

    public class PollingException : Exception
    {
        public PollingExceptionType type_exception;
        public string message_exception;

        public PollingException()
        {
            //
            // TODO: aggiungere qui la logica del costruttore
            //
        }

        public PollingException(PollingExceptionType tipo, string messaggio)
        {
            this.type_exception = tipo;
            this.message_exception = messaggio;
            //da implementare successivamente
        }

    }

    public class PollingExceptionHandler
    {

        public PollingExceptionHandler()
        { }

        public static string Esegui(Exception ex)
        {
            string risultato = "";
            string tipo_errore = ex.GetType().ToString();
            switch (tipo_errore)
            {
                case "Database.PollingException":
                    risultato = GestisciPollingException((PollingException)ex);
                    break;

                default:
                    risultato = ex.Message;
                    break;
            }


            //da implementare successivamente

            return risultato;
        }

        private static string GestisciPollingException(PollingException ex)
        {
            string risultato = ex.Message;

            risultato = ex.type_exception.ToString() + " " + ex.message_exception;

            return risultato;
        }


    }
}