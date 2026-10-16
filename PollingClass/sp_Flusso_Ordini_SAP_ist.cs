using System;
using Database;

using System.Data;
using Microsoft.Data.SqlClient;
using Database.Base;
using System.Data.SqlTypes;

namespace PollingClass
{
    public class sp_Flusso_Ordini_SAP_ist
    {
        #region ATTRIBUTI PUBBLICI


        //parametri input
        public static int @pi_istituto;
        

        //parametri output
        public static string @po_valoreuscita;        
                
        //return value
        public static int returnValue;


        #endregion

        #region COSTRUTTORI

        public sp_Flusso_Ordini_SAP_ist()
        {

        }

        #endregion

        #region METODI STATICI


        public static string Esegui(DatabaseInfo db,string istituto)
        {
            SqlCommand command = new SqlCommand();
            SqlParameter parameter;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[Flusso_Ordini_SAP_ist]";

            command.Connection = db.connessione;
            //command.Transaction = InfoSessioneUtente.DatabaseInterface.transazione;

            try
            {
                //parametri Input
                parameter = command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@pi_istituto", System.Data.SqlDbType.Int));
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = istituto;

                ////parametri Input
                //parameter = command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@pi_guid", System.Data.SqlDbType.NVarChar,40));
                //parameter.Direction = ParameterDirection.Input;
                //parameter.Value = guid;

                //parametri Output
                parameter = command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@po_valoreuscita", System.Data.SqlDbType.NVarChar, 40, System.Data.ParameterDirection.Output, false, ((System.Byte)(10)), ((System.Byte)(0)), "", System.Data.DataRowVersion.Current, null));
                parameter.Direction = ParameterDirection.Output;

                //RETURN_VALUE
                parameter = command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@RETURN_VALUE", System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.ReturnValue, false, ((System.Byte)(10)), ((System.Byte)(0)), "", System.Data.DataRowVersion.Current, null));
                parameter.Direction = ParameterDirection.ReturnValue;

                command.CommandTimeout = 3000;

                SqlDataReader myReader = command.ExecuteReader();
                
                returnValue = (int)command.Parameters["@RETURN_VALUE"].Value;
                if (returnValue == 0)
                {
                    //@po_valoreuscita = (double)command.Parameters["@po_prezzo"].Value;

                }
                else
                {
                    ////parametri output di errore di livello
                 }
                myReader.Close();


            }
            catch (Exception ex)
            {
                PollingExceptionHandler.Esegui(ex);
            }
            return @po_valoreuscita;
        }

        

        #endregion

    }
}


