using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    public class SqlClientAdapterTests
    {
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
