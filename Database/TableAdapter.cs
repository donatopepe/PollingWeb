using System;

using Microsoft.Data.SqlClient;
using System.Data;
using Database.Base;


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

        // Provider seam: all generated adapters remain on the single Microsoft.Data.SqlClient provider.
        protected static ISqlClientFactory SqlClientFactory { get; set; } = new MicrosoftSqlClientFactory();

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


        protected string BuildWhere(System.Collections.ArrayList filters)
        {
            adapter.SelectCommand.Parameters.Clear();
            return FiltroCampi.BuildWhere(filters, adapter.SelectCommand);
        }

        protected string BuildWhereClause(System.Collections.ArrayList filters, string selectSql)
        {
            var where = BuildWhere(filters);
            return selectSql + (where.Length == 0 ? string.Empty : " where " + where);
        }

        public SqlDataAdapter BuildCommand(SqlDataAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            using (var build = SqlClientFactory.CreateCommandBuilder(adapter))
            {
            build.RefreshSchema();

            var timeout = adapter.SelectCommand.CommandTimeout > 0
                ? adapter.SelectCommand.CommandTimeout
                : DatabaseInfo.DefaultCommandTimeoutSeconds;
            if (build.DataAdapter.SelectCommand != null) adapter.SelectCommand.CommandTimeout = timeout;
            if (build.DataAdapter.InsertCommand != null)
            {
                adapter.InsertCommand = build.GetInsertCommand();
                adapter.InsertCommand.CommandTimeout = timeout;
            }
            if (build.DataAdapter.UpdateCommand != null)
            {
                adapter.UpdateCommand = build.GetUpdateCommand();
                adapter.UpdateCommand.CommandTimeout = timeout;
            }
            if (build.DataAdapter.DeleteCommand != null)
            {
                adapter.DeleteCommand = build.GetDeleteCommand();
                adapter.DeleteCommand.CommandTimeout = timeout;
            }
            return adapter;
            }
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
