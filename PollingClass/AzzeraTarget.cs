using System;

using System.Data;
using System.Data.SqlClient;
using Database.Base;
using System.Data.SqlTypes;

namespace PollingClass
{
    public class AzzeraTargetSC00G100
    {
        #region ATTRIBUTI PUBBLICI


        //parametri input
        //public static string codazi;
        

        //parametri output
        public static string @po_valoreuscita;        
                
        //return value
        public static int returnValue;


        #endregion

        #region COSTRUTTORI

        public AzzeraTargetSC00G100()
        {

        }

        #endregion

        #region METODI STATICI


        public static string Esegui(DatabaseInfo db,string istituto)
        {
            SqlDataReader myReader = null;
            SqlCommand command = new SqlCommand();
            SqlParameter parameter;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "[AzzeraTargetSC00G100]";

            command.Connection = db.connessione;
            //command.Transaction = InfoSessioneUtente.DatabaseInterface.transazione;

            try
            {
                //parametri Input
                parameter = command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@pi_istituto", System.Data.SqlDbType.Int));
                parameter.Direction = ParameterDirection.Input;
                parameter.Value = istituto;

                //parametri Output
                parameter = command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@po_valoreuscita", System.Data.SqlDbType.NVarChar, 30, System.Data.ParameterDirection.Output, false, ((System.Byte)(10)), ((System.Byte)(0)), "", System.Data.DataRowVersion.Current, null));
                parameter.Direction = ParameterDirection.Output;

                //RETURN_VALUE
                parameter = command.Parameters.Add(new System.Data.SqlClient.SqlParameter("@RETURN_VALUE", System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.ReturnValue, false, ((System.Byte)(10)), ((System.Byte)(0)), "", System.Data.DataRowVersion.Current, null));
                parameter.Direction = ParameterDirection.ReturnValue;

                command.CommandTimeout = 3000;

                myReader = command.ExecuteReader();
                
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
                if  (!myReader.IsClosed)                
                    myReader.Close();                

                Console.WriteLine(ex.Message);
            }
            return @po_valoreuscita;
        }

        

        #endregion

    }
}


