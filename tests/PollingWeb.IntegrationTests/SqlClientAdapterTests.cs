using System;
using System.Collections;
using Database;
using Database.TableInterface;
using Microsoft.Data.SqlClient;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    public class SqlClientAdapterTests
    {
        [Fact]
        public void TableAdapter_select_command_keeps_injection_payload_as_data()
        {
            var adapter = new ExposedTableAdapter();
            adapter.Initialize();
            var filter = new FiltroCampi { Campo = "IdProcesso", Valore = "' OR 1=1 --", Tipo = "C" };
            var sql = adapter.Where(new ArrayList { filter }, "SELECT IdProcesso FROM dbo.TaskService");

            Assert.Contains("@p0", sql);
            Assert.DoesNotContain("OR 1=1", sql.ToUpperInvariant());
            Assert.Equal("' OR 1=1 --", adapter.Command.Parameters["@p0"].Value);
        }

        private sealed class ExposedTableAdapter : TableAdapter
        {
            public SqlCommand Command => adapter.SelectCommand;

            public void Initialize()
            {
                adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand();
            }

            public string Where(ArrayList filters, string selectSql) => BuildWhereClause(filters, selectSql);
        }

        [RequiresDatabaseFact]
        public void SqlDataAdapter_and_CommandBuilder_round_trip_synthetic_row()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString()))
            {
                connection.Open();
                try
                {
                    using (var create = new SqlCommand(
                        "CREATE TABLE dbo.[__PollingWebSqlClientProbe] (Id int NOT NULL PRIMARY KEY, Value nvarchar(40) NOT NULL)", connection))
                        create.ExecuteNonQuery();

                    var adapter = new SqlDataAdapter("SELECT Id, Value FROM dbo.[__PollingWebSqlClientProbe]", connection);
                    using (var builder = new SqlCommandBuilder(adapter))
                    {
                        var data = new System.Data.DataTable();
                        adapter.Fill(data);
                        var row = data.NewRow();
                        row["Id"] = 1;
                        row["Value"] = "synthetic-probe";
                        data.Rows.Add(row);
                        adapter.Update(data);
                        data.Clear();
                        adapter.Fill(data);
                        Assert.Single(data.Rows);
                        Assert.Equal("synthetic-probe", data.Rows[0]["Value"]);

                        data.Rows[0]["Value"] = "updated-probe";
                        adapter.Update(data);
                        data.Rows[0].Delete();
                        adapter.Update(data);
                        data.Clear();
                        adapter.Fill(data);
                        Assert.Empty(data.Rows);
                    }
                }
                finally
                {
                    using (var drop = new SqlCommand(
                        "IF OBJECT_ID(N'dbo.__PollingWebSqlClientProbe', N'U') IS NOT NULL DROP TABLE dbo.[__PollingWebSqlClientProbe]", connection))
                        drop.ExecuteNonQuery();
                }
            }
        }
    }
}
