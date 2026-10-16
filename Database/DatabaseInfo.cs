using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using Database.Configuration;
using Microsoft.Data.SqlClient;

namespace Database.Base
{
    /// <summary>Owns one database connection and transaction; instances share no mutable state.</summary>
    public sealed class DatabaseInfo : IDisposable
    {
        public const int DefaultCommandTimeoutSeconds = 30;
        private readonly int commandTimeoutSeconds;
        private bool disposed;

        public SqlConnection connessione { get; private set; }
        public SqlTransaction transazione { get; private set; }
        public int CommandTimeoutSeconds => commandTimeoutSeconds;

        public DatabaseInfo() : this(new AppConfig(), false) { }
        public DatabaseInfo(bool eprocurement) : this(new AppConfig(), eprocurement) { }

        public DatabaseInfo(IAppConfig configuration, bool eprocurement = false)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var connectionName = eprocurement ? "PollingEprocurement" : "PollingConnectionString";
            var connectionString = configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationErrorsException("Connection string '" + connectionName + "' is not configured.");

            int timeout;
            var timeoutValue = configuration.GetValue("POLLING_DB_COMMAND_TIMEOUT_SECONDS");
            commandTimeoutSeconds = int.TryParse(timeoutValue, out timeout) && timeout > 0
                ? timeout : DefaultCommandTimeoutSeconds;
            connessione = new SqlConnection(connectionString);
        }

        public void IniziaSessione()
        {
            ThrowIfDisposed();
            connessione.Open();
        }

        public void TerminaSessione()
        {
            ThrowIfDisposed();
            if (transazione != null)
            {
                transazione.Dispose();
                transazione = null;
            }
            if (connessione.State != ConnectionState.Closed) connessione.Close();
        }

        public void IniziaTransazione()
        {
            ThrowIfDisposed();
            if (connessione.State != ConnectionState.Open) connessione.Open();
            if (transazione != null) throw new InvalidOperationException("A transaction is already active.");
            transazione = connessione.BeginTransaction();
        }

        public void AnnullaTransazione()
        {
            ThrowIfDisposed();
            if (transazione == null) return;
            transazione.Rollback();
            transazione.Dispose();
            transazione = null;
        }

        public void ConfermaTransazione()
        {
            ThrowIfDisposed();
            if (transazione == null) throw new InvalidOperationException("No active transaction.");
            transazione.Commit();
            transazione.Dispose();
            transazione = null;
        }

        public void Dispose()
        {
            if (disposed) return;
            if (transazione != null)
            {
                try { transazione.Rollback(); }
                catch (DbException) { }
                transazione.Dispose();
                transazione = null;
            }
            if (connessione != null) connessione.Dispose();
            disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(DatabaseInfo));
        }
    }
}
