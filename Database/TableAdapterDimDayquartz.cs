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
    public class TableAdapterDimDayquartz : TableAdapter
    {
        #region ATTRIBUTI PRIVATI


        //connessione
        private DatabaseInfo databaseInterface;


        //definito nella superclasse
        //private SqlDataAdapter adapter;

        private dtsTabelle dataset;

        private SqlDataReader reader;

        private string NomeTabella = "DimDayquartz";

        #endregion


        #region ATTRIBUTI PUBBLICI


        //colonne
        public string DayQuartz = "";
        public string Descrizione = "";
       


        #endregion


        #region COSTRUTTORI

        public TableAdapterDimDayquartz(DatabaseInfo databaseInterface, dtsTabelle dataset)
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

            this.adapter.SelectCommand.Parameters.Clear();
            var where = FiltroCampi.BuildWhere(array, this.adapter.SelectCommand);
            this.adapter.SelectCommand.CommandText = select + (where.Length == 0 ? string.Empty : " where " + where);

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

            //this.Codice = "";          

            if (esisteRecord)
            {
                this.DayQuartz =(reader["DayQuartz"].ToString());
                this.Descrizione = reader["Descrizione"].ToString();
                
               
            }
        }




        public bool Read(ArrayList array)
        {
            bool esisteRecord;

            string select = "Select * from " + this.NomeTabella + " with (nolock) ";

            this.adapter.SelectCommand.Parameters.Clear();
            var where = FiltroCampi.BuildWhere(array, this.adapter.SelectCommand);
            this.adapter.SelectCommand.CommandText = select + (where.Length == 0 ? string.Empty : " where " + where);

            this.reader = this.adapter.SelectCommand.ExecuteReader();

            esisteRecord = this.reader.Read();

            this.AssegnaAttributi(esisteRecord);

            reader.Close();

            return esisteRecord;
        }



        #endregion



    }
}
