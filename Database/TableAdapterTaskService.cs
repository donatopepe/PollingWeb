using System;
using System.Collections;
using Microsoft.Data.SqlClient;
using Database.Base;
using Database;
using System.Data;


namespace Database.TableInterface
{
    /// <summary>
    /// Descrizione di riepilogo per TableAdapterConprites.
    /// </summary>
    public class TableAdapterTaskService : TableAdapter
    {
        #region ATTRIBUTI PRIVATI


        //connessione
        private DatabaseInfo databaseInterface;


        //definito nella superclasse
        //private SqlDataAdapter adapter;

        private dtsTabelle dataset;

        private SqlDataReader reader;

        private string NomeTabella = "TaskService";

        #endregion


        #region ATTRIBUTI PUBBLICI


        //colonne
        public Int16 Istituto = 0;
        public string IdProcesso = "";
        public string Descrizione = "";
        public string Path_Source = "";
        public string File_Source = "";
        public DateTime Orario_Check_File = DateTime.MinValue;
        public string Day_Check_File = "";
        public Int16 Polling_Intervall = 0;
        public Int16 Cutoff = 0;
        public string Mail_Mittente = "";
        public string Mail_Failure = "";
        public string Path_Archiviazione = "";
        public string Mail_success = "";



        #endregion


        #region COSTRUTTORI

        public TableAdapterTaskService(DatabaseInfo databaseInterface, dtsTabelle dataset)
        {

            // ASSEGNAZIONE AI MEMBRI DELLA CLASSE

            this.databaseInterface = databaseInterface;

            this.adapter = CreaSqlDataAdapter();

            this.dataset = dataset;

        }


        #endregion


        #region METODI DI SUPPORTO AI COSTRUTTORI



        private SqlDataAdapter CreaSqlDataAdapter()
        {
            Microsoft.Data.SqlClient.SqlDataAdapter sqlDataAdapter1;
            sqlDataAdapter1 = new Microsoft.Data.SqlClient.SqlDataAdapter();

            Microsoft.Data.SqlClient.SqlCommand sqlSelectCommand1;
            Microsoft.Data.SqlClient.SqlCommand sqlInsertCommand1;
            Microsoft.Data.SqlClient.SqlCommand sqlUpdateCommand1;
            Microsoft.Data.SqlClient.SqlCommand sqlDeleteCommand1;

            sqlSelectCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            sqlInsertCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            sqlUpdateCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            sqlDeleteCommand1 = new Microsoft.Data.SqlClient.SqlCommand();
            // 
            // sqlDataAdapter1
            // 
            sqlDataAdapter1.DeleteCommand = sqlDeleteCommand1;
            sqlDataAdapter1.InsertCommand = sqlInsertCommand1;
            sqlDataAdapter1.SelectCommand = sqlSelectCommand1;
            sqlDataAdapter1.UpdateCommand = sqlUpdateCommand1;
            // 
            // sqlSelectCommand1
            // 
            sqlSelectCommand1.CommandText = "SELECT * FROM " + this.NomeTabella + " with (nolock)";



            //ASSEGNO CONNESSIONE  AI SQLCOMMAND
            sqlDataAdapter1.SelectCommand.Connection = databaseInterface.connessione;
            sqlDataAdapter1.InsertCommand.Connection = databaseInterface.connessione;
            sqlDataAdapter1.UpdateCommand.Connection = databaseInterface.connessione;
            sqlDataAdapter1.DeleteCommand.Connection = databaseInterface.connessione;


            // RESTITUISCO IL DATA ADAPTER

            return this.BuildCommand(sqlDataAdapter1);
        }


        #endregion


        #region METODI PER POPOLARE IL DATASET


        public int Load(ArrayList array)
        {

            string select = "Select * from " + this.NomeTabella + " with (nolock) ";

            string where = this.BuildWhere(array);
            where = where != "" ? " where " + where : "";

            this.adapter.SelectCommand.CommandText = select + where;

            this.FillDataTable();

            return (this.dataset.Tables[this.NomeTabella].Rows.Count);
        }


        public int Load(string tabelle, string where, string campi)
        {

            string select = "Select " + campi + " from " + tabelle + " with (nolock) ";


            where = where != "" ? "where " + where : where;

            this.adapter.SelectCommand.CommandText = select + where;

            this.FillDataTable();

            return (this.dataset.Tables[this.NomeTabella].Rows.Count);
        }



        #endregion


        #region METODI DI SUPPORTO PER POPOLARE IL DATASET



        private void FillDataTable()
        {
            try
            {
                this.dataset.Tables[this.NomeTabella].Rows.Clear();

                this.adapter.Fill(this.dataset.Tables[this.NomeTabella]);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




        #endregion


        #region METODI PER AGGIORNARE IL DATABASE


        public void SaveDataTable()
        {

            DataTable changesDataTable = this.dataset.Tables[this.NomeTabella].GetChanges();

            try
            {
                if (changesDataTable != null)
                {
                    this.adapter.Update(changesDataTable);

                    //this.dataset.Tables[this.NomeTabella].AcceptChanges();
                }
            }
            catch (Exception ex)
            {
                this.dataset.Tables[this.NomeTabella].RejectChanges();
                throw ex;
            }
        }

        #endregion


        #region CONFERMA/ANNULLA MODIFICHE SUL DATASET


        public void ConfermaDataset()
        {
            try
            {
                DataTable changesDataTable = this.dataset.Tables[this.NomeTabella].GetChanges();

                if (changesDataTable != null)
                    this.dataset.Tables[this.NomeTabella].AcceptChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void AnnullaDataset()
        {
            this.dataset.Tables[this.NomeTabella].RejectChanges();
        }



        #endregion


        #region METODI PER LEGGERE UNO O PIU' RECORD



        private void AssegnaAttributi(bool esisteRecord)
        {
                     

            if (esisteRecord)
            {
                this.Istituto = Convert.ToInt16(reader["Istituto"].ToString());
                this.IdProcesso = (reader["IdProcesso"].ToString());
                this.Descrizione = reader["Descrizione"].ToString();
                this.Path_Source = reader["Path_Source"].ToString();
                this.File_Source = reader["File_Source"].ToString();
                this.Orario_Check_File = Convert.ToDateTime(reader["Orario_Check_File"].ToString());
                this.Day_Check_File = reader["Day_Check_File"].ToString();
                this.Polling_Intervall = Convert.ToInt16(reader["Polling_Intervall"].ToString());
                this.Cutoff = Convert.ToInt16(reader["Cutoff"].ToString());
                this.Mail_Mittente = reader["Mail_Mittente"].ToString();
                this.Mail_Failure = reader["Mail_Failure"].ToString();
                this.Path_Archiviazione = reader["Path_Archiviazione"].ToString();
                this.Mail_success = reader["Mail_success"].ToString();
            }
        }




        public bool Read(ArrayList array)
        {
            bool esisteRecord;

            string select = "Select * from " + this.NomeTabella + " with (nolock) ";

            string where = this.BuildWhere(array);
            where = where != "" ? " where " + where : "";

            this.adapter.SelectCommand.CommandText = select + where;

            this.reader = this.adapter.SelectCommand.ExecuteReader();

            esisteRecord = this.reader.Read();

            this.AssegnaAttributi(esisteRecord);

            reader.Close();

            return esisteRecord;
        }



        #endregion



    }
}
