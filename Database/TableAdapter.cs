using System;

using System.Data.SqlClient;
using System.Data;


namespace Database.TableInterface
{
    public class TableAdapter
    {
        private string NomeTabella = "";

        public TableAdapter()
        {
        }

        public TableAdapter(string nomeTabella)
        {
            this.NomeTabella = nomeTabella;
        }

        //data adapter
        protected SqlDataAdapter adapter;


        public void AssegnaTransazione(SqlTransaction trans)
        {
            adapter.SelectCommand.Transaction = trans;
            adapter.InsertCommand.Transaction = trans;
            adapter.UpdateCommand.Transaction = trans;
            adapter.DeleteCommand.Transaction = trans;
        }




        #region METODI DI SUPPORTO


        public SqlDataAdapter BuildCommand(SqlDataAdapter adapter)
        {
            try
            {
                SqlCommandBuilder build = new SqlCommandBuilder(adapter);

                build.RefreshSchema();

                if (build.DataAdapter.SelectCommand != null)
                    adapter.SelectCommand.CommandTimeout = Convert.ToInt32(Database.Base.DatabaseInfo.timeOut);

                if (build.DataAdapter.InsertCommand != null)
                {
                    adapter.InsertCommand = build.GetInsertCommand();
                    adapter.SelectCommand.CommandTimeout = Convert.ToInt32(Database.Base.DatabaseInfo.timeOut);
                }

                if (build.DataAdapter.UpdateCommand != null)
                {
                    adapter.UpdateCommand = build.GetUpdateCommand();
                    adapter.SelectCommand.CommandTimeout = Convert.ToInt32(Database.Base.DatabaseInfo.timeOut);
                }

                if (build.DataAdapter.DeleteCommand != null)
                {
                    adapter.DeleteCommand = build.GetDeleteCommand();
                    adapter.SelectCommand.CommandTimeout = Convert.ToInt32(Database.Base.DatabaseInfo.timeOut);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return adapter;
        }


        #endregion


        // Questa proprietà restituisce *direttamente* la DataTable a cui
        // siamo agganciati. In questo modo non abbiamo bisogno di usare
        // un dataset tipizzato.
        public virtual DataTable DataTable
        {
            // Questa non dovrebbe *mai* essere chiamata
            get
            {
                return null;
            }
        }


        #region METODI PER POPOLARE IL DATASET

        public virtual int Load(string codazi)
        {
            // Questa non dovrebbe *mai* essere chiamata
            return -1;
        }




        #endregion


    }
}
