using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    /// <summary>
    /// Test del <b>modello di privilegi minimi</b> introdotto in <c>T0.8</c>.
    ///
    /// <para>
    /// Verificano sia ciò che l'utente applicativo deve poter fare, sia — ed è la parte che
    /// conta — ciò che <b>non</b> deve poter fare. Un controllo di permesso che passa perché
    /// non è mai stato esercitato non è un controllo: è una speranza.
    /// </para>
    /// <para>
    /// Girano con le credenziali dell'utente applicativo, non con <c>EXECUTE AS</c>: è
    /// l'unico modo per verificare che quelle credenziali funzionino davvero e per
    /// attraversare i confini fra database come fa l'applicazione.
    /// </para>
    /// </summary>
    public class LeastPrivilegeTests
    {
        private static void Esegui(string database, string sql, Action<SqlCommand> configura = null)
        {
            using (var conn = new SqlConnection(TestDatabase.AppConnectionString(database)))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (configura != null)
                    {
                        configura(cmd);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static T Leggi<T>(string database, string sql)
        {
            using (var conn = new SqlConnection(TestDatabase.AppConnectionString(database)))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return (T)Convert.ChangeType(cmd.ExecuteScalar(), typeof(T));
                }
            }
        }

        [RequiresAppUserFact]
        public void Puo_leggere_e_scrivere_nel_database_applicativo()
        {
            Leggi<int>("Polling", "SELECT TOP 1 1 FROM dbo.TaskService");
            Leggi<int>("Polling", "SELECT COUNT(*) FROM dbo.ViewTaskService");

            // Scrittura in transazione annullata: il test non deve lasciare traccia.
            Esegui("Polling",
                "BEGIN TRAN; INSERT INTO dbo.Tool (Name) VALUES (N'perm-test'); ROLLBACK;");
        }

        [RequiresAppUserFact]
        public void Puo_eseguire_una_stored_procedure()
        {
            using (var conn = new SqlConnection(TestDatabase.AppConnectionString("Polling")))
            {
                conn.Open();
                using (var cmd = new SqlCommand("dbo.AzzeraTargetSC00G100", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@pi_istituto", 1));
                    var uscita = cmd.Parameters.Add(new SqlParameter("@po_valoreuscita", System.Data.SqlDbType.NVarChar, 30));
                    uscita.Direction = System.Data.ParameterDirection.Output;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [RequiresAppUserFact]
        public void Puo_scrivere_in_eProcurement_come_fanno_le_procedure()
        {
            // Le stored procedure del database applicativo scrivono in eProcurement: se
            // all'utente mancasse il permesso lì, i flussi si romperebbero solo a runtime.
            Esegui("eProcurement",
                "BEGIN TRAN; INSERT INTO dbo.Log_ODA_SAP (Istituto, Data_Put) VALUES (1, GETDATE()); ROLLBACK;");
        }

        [RequiresAppUserFact]
        public void Puo_accedere_al_job_store_Quartz()
        {
            Leggi<int>("Quartz", "SELECT COUNT(*) FROM dbo.QRTZ_LOCKS");
        }

        [RequiresAppUserFact]
        public void NON_puo_eseguire_DDL_nel_database_applicativo()
        {
            Assert.Throws<SqlException>(() =>
                Esegui("Polling", "CREATE TABLE dbo.__pollingweb_should_fail (x int)"));
        }

        [RequiresAppUserFact]
        public void NON_puo_eseguire_DDL_in_eProcurement()
        {
            Assert.Throws<SqlException>(() =>
                Esegui("eProcurement", "CREATE TABLE dbo.__pollingweb_should_fail (x int)"));
        }

        [RequiresAppUserFact]
        public void NON_puo_eseguire_DDL_in_Quartz()
        {
            Assert.Throws<SqlException>(() =>
                Esegui("Quartz", "CREATE TABLE dbo.__pollingweb_should_fail (x int)"));
        }

        [RequiresAppUserFact]
        public void NON_e_db_owner()
        {
            Assert.Equal(0, Leggi<int>("Polling", "SELECT IS_ROLEMEMBER('db_owner')"));
            Assert.Equal(0, Leggi<int>("eProcurement", "SELECT IS_ROLEMEMBER('db_owner')"));
            Assert.Equal(0, Leggi<int>("Quartz", "SELECT IS_ROLEMEMBER('db_owner')"));
        }

        [RequiresAppUserFact]
        public void NON_e_sysadmin()
        {
            Assert.Equal(0, Leggi<int>("master", "SELECT IS_SRVROLEMEMBER('sysadmin')"));
        }
    }
}
