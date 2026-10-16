using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections;

using Database.Base;
using Database;


namespace Database.TableInterface
{
    public class Aggset : TableAdapter
    {
        //private SqlDataReader reader;
        //private string NomeTabella;
        private DatabaseInfo databaseInterface;
        //public static SqlDataReader my_reader;
        public SqlDataReader campi_reader;
        private DataSet dataset;

        //public  DataTable lista_campi;
        public static DataTable campitabella;

        #region COSTRUTTORI
        public Aggset(DatabaseInfo databaseInterface, DataSet datasetPersonalizzata)
        {

            this.databaseInterface = databaseInterface;

            this.dataset = (DataSet)datasetPersonalizzata;
        }
        #endregion





        public static SqlDataReader Read(string nometabella, string where, string campi, DatabaseInfo databaseinterface)
        {

            SqlDataAdapter adapter = new SqlDataAdapter();
            //SqlDataReader reader = new SqlDataReader();
            SqlCommand sel = new SqlCommand();
            adapter.SelectCommand = sel;
            adapter.SelectCommand.Connection = databaseinterface.connessione;
            adapter.SelectCommand.CommandTimeout = 180; // default è 30 secondi
            SqlCommandBuilder build = new SqlCommandBuilder(adapter);
            build.RefreshSchema();

            campi = campi == "" ? "*" : campi;
            where = where != "" ? " " + where : "";

            string select = "Select " + campi + " from " + nometabella;

            adapter.SelectCommand.CommandText = select + where;

            SqlDataReader reader = adapter.SelectCommand.ExecuteReader();
            //reader.Read();

            //reader.Close();

            return reader;



        }


        public static int Load(string nometabella, string where, string campi, DatabaseInfo databaseinterface, DataSet datasetPersonalizzata)
        {

            SqlDataAdapter adapter = new SqlDataAdapter();
            SqlCommand sel = new SqlCommand();
            adapter.SelectCommand = sel;
            adapter.SelectCommand.Connection = databaseinterface.connessione;
            adapter.SelectCommand.CommandTimeout = 180; // default è 30 secondi
            SqlCommandBuilder build = new SqlCommandBuilder(adapter);
            build.RefreshSchema();

            DataSet dataset = (DataSet)datasetPersonalizzata;



            campi = campi == "" ? "*" : campi;
            where = where != "" ? " " + where : "";

            string select = "Select " + campi + " from " + nometabella;

            adapter.SelectCommand.CommandText = select + where;

            FillDataTable(dataset, adapter, nometabella);

            campitabella = dataset.Tables[nometabella];


            return (dataset.Tables[nometabella].Rows.Count);
        }

        static void FillDataTable(DataSet dataset, SqlDataAdapter adapter, string NomeTabella)
        {
            try
            {
                dataset.Tables[NomeTabella].Rows.Clear();

                adapter.Fill(dataset.Tables[NomeTabella]);

            }
            catch
            {
                throw;
            }
        }


        public static int Eseguicomando(string comando, DatabaseInfo databaseinterface)
        {
            try
            {
                int righe = 0;

                SqlDataAdapter adapter = new SqlDataAdapter();

                SqlCommand sel = new SqlCommand();
                adapter.SelectCommand = sel;
                adapter.SelectCommand.Connection = databaseinterface.connessione;
                adapter.SelectCommand.CommandTimeout = 180; // default è 30 secondi
                adapter.SelectCommand.Transaction = databaseinterface.transazione;

                SqlCommandBuilder build = new SqlCommandBuilder(adapter);
                build.RefreshSchema();

                adapter.SelectCommand.CommandText = comando;

                righe = adapter.SelectCommand.ExecuteNonQuery();


                return righe;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Eseguicomando failed.", ex);
            }

        }
        public static DataSet EseguiQuery(string nome, string comando, DatabaseInfo databaseinterface)
        {
            try
            {
                string queryString = comando;
                SqlDataAdapter adapter = new SqlDataAdapter(queryString, databaseinterface.connessione);

                DataSet data = new DataSet();
                adapter.Fill(data, nome);
                return data;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("EseguiQuery failed.", ex);
            }

        }
    }
}
