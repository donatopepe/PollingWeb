using Microsoft.Data.SqlClient;

namespace Database.Base
{
    /// <summary>Factory boundary for the single supported SQL Server provider.</summary>
    public interface ISqlClientFactory
    {
        SqlConnection CreateConnection(string connectionString);
        SqlDataAdapter CreateDataAdapter();
        SqlCommandBuilder CreateCommandBuilder(SqlDataAdapter adapter);
    }

    public sealed class MicrosoftSqlClientFactory : ISqlClientFactory
    {
        public SqlConnection CreateConnection(string connectionString) => new SqlConnection(connectionString);
        public SqlDataAdapter CreateDataAdapter() => new SqlDataAdapter();
        public SqlCommandBuilder CreateCommandBuilder(SqlDataAdapter adapter) => new SqlCommandBuilder(adapter);
    }
}
