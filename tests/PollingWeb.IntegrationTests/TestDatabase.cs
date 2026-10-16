using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    /// <summary>
    /// Configurazione dei test di integrazione e meccanismo di <b>salto automatico</b>.
    ///
    /// <para>
    /// Le credenziali e l'indirizzo arrivano dalle stesse variabili d'ambiente usate da
    /// <c>scripts/db-deploy.sh</c>: nessuna credenziale nuova nel repository, e chi prepara
    /// l'istanza usa un solo insieme di variabili.
    /// </para>
    /// <para>
    /// La stringa di connessione include <c>TrustServerCertificate=True</c>. Non è una
    /// scorciatoia: è la conseguenza documentata come fatto <c>P15</c> — dal provider
    /// <c>Microsoft.Data.SqlClient</c> 5.x <c>Encrypt</c> è attivo per default e contro un
    /// certificato generato dal container la connessione fallisce con un errore di handshake
    /// pre-login, il cui messaggio non dice cosa manca.
    /// </para>
    /// </summary>
    internal static class TestDatabase
    {
        private static readonly Lazy<string> Disponibilita = new Lazy<string>(VerificaDisponibilita);

        internal static string Server =>
            Environment.GetEnvironmentVariable("POLLING_DB_SERVER") ?? "localhost,14330";

        internal static string User =>
            Environment.GetEnvironmentVariable("POLLING_DB_USER") ?? "sa";

        internal static string Password =>
            Environment.GetEnvironmentVariable("POLLING_DB_PASSWORD");

        internal static string AppUser =>
            Environment.GetEnvironmentVariable("POLLING_APP_LOGIN") ?? "PollingApp";

        internal static string AppPassword =>
            Environment.GetEnvironmentVariable("POLLING_APP_PASSWORD");

        /// <summary>Restituisce null se i test possono girare, altrimenti il motivo del salto.</summary>
        internal static string MotivoNonDisponibile => Disponibilita.Value;

        /// <summary>Stringa di connessione al database applicativo, come utente amministrativo.</summary>
        internal static string ConnectionString(string database = "Polling") =>
            Costruisci(User, Password, database);

        /// <summary>Stringa di connessione come utente applicativo a privilegi minimi.</summary>
        internal static string AppConnectionString(string database = "Polling") =>
            Costruisci(AppUser, AppPassword, database);

        internal static string QuartzConnectionString(string database = "Quartz")
        {
            var explicitQuartz = Environment.GetEnvironmentVariable("POLLING_QUARTZ_CONNECTION");
            return string.IsNullOrWhiteSpace(explicitQuartz)
                ? AppConnectionString(database)
                : explicitQuartz;
        }

        private static string Costruisci(string user, string password, string database) =>
            $"Server={Server};Database={database};User Id={user};$1=[REDACTED];" +
            "Connect Timeout=5;TrustServerCertificate=True;";

        private static string VerificaDisponibilita()
        {
            if (string.IsNullOrEmpty(Password))
            {
                return "POLLING_DB_PASSWORD non impostata: test di integrazione saltati. " +
                       "Preparare l'istanza con ./scripts/test-integration.sh";
            }

            try
            {
                using (var conn = new SqlConnection(
                    $"Server={Server};Database=master;User Id={User};$1=[REDACTED];" +
                    "Connect Timeout=5;TrustServerCertificate=True;"))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT DB_ID(N'Polling')", conn))
                    {
                        var esito = cmd.ExecuteScalar();
                        if (esito == null || esito == DBNull.Value)
                        {
                            return "il database 'Polling' non esiste: eseguire scripts/db-deploy.sh";
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return $"SQL Server non raggiungibile su {Server} ({ex.GetType().Name}: " +
                       $"{ex.Message.Split('\n')[0]}): test di integrazione saltati";
            }
        }
    }

    /// <summary>
    /// <c>[Fact]</c> che si salta da solo quando l'istanza non è disponibile.
    /// <para>
    /// Il controllo avviene alla scoperta dei test (costruttore dell'attributo) ed è
    /// memorizzato nella <c>Lazy</c> di <see cref="TestDatabase"/>: si paga una sola
    /// connessione, non una per test.
    /// </para>
    /// </summary>
    public sealed class RequiresDatabaseFactAttribute : FactAttribute
    {
        public RequiresDatabaseFactAttribute()
        {
            var motivo = TestDatabase.MotivoNonDisponibile;
            if (motivo != null)
            {
                Skip = motivo;
            }
        }
    }

    /// <summary>Come <see cref="RequiresDatabaseFactAttribute"/>, ma anche con l'utente applicativo.</summary>
    public sealed class RequiresAppUserFactAttribute : FactAttribute
    {
        public RequiresAppUserFactAttribute()
        {
            var motivo = TestDatabase.MotivoNonDisponibile
                         ?? (string.IsNullOrEmpty(TestDatabase.AppPassword)
                             ? "POLLING_APP_PASSWORD non impostata: test dei permessi saltati"
                             : null);
            if (motivo != null)
            {
                Skip = motivo;
            }
        }
    }
}
