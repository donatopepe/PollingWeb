using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Xunit;

namespace PollingWeb.IntegrationTests
{
    /// <summary>
    /// Test del <b>contratto dello schema</b>: fissano ciò che il deploy deve produrre.
    ///
    /// <para>
    /// Sono i test che danno valore al lavoro di <c>T0.7</c> (fonte unica dello schema) e
    /// <c>T0.12</c> (schema <c>eProcurement</c> recuperato in sola DDL). I numeri attesi non
    /// sono stimati: sono quelli verificati oggetto per oggetto durante quei task, e
    /// corrispondono ai file versionati in <c>sql/</c>. Se qualcuno modifica lo schema senza
    /// aggiornare il deploy, o viceversa, questi test lo dicono subito.
    /// </para>
    /// <para>
    /// Coprono anche l'<b>assenza</b> di ciò che è noto mancare: lo schema del database
    /// <c>eProcurement</c> era vuoto prima di <c>T0.12</c>, quindi il conteggio non è
    /// decorativo.
    /// </para>
    /// </summary>
    public class SchemaContractTests
    {
        private static int Conteggio(string database, string sql)
        {
            using (var conn = new SqlConnection(TestDatabase.ConnectionString(database)))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static List<string> Nomi(string database, string sql)
        {
            var risultato = new List<string>();
            using (var conn = new SqlConnection(TestDatabase.ConnectionString(database)))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        risultato.Add(reader.GetString(0));
                    }
                }
            }
            return risultato;
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_executes_with_eProcurement_archive_target()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand("BEGIN TRANSACTION; BEGIN TRY DECLARE @output varchar(40); EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;", connection))
                    command.ExecuteNonQuery();
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_ist_executes_with_synthetic_request_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier = NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID, Progressivo, PageID, Matricola, IstID, Istituto, Status, " +
                    "Oggetto, DataModifica, CFor, GruppoAcquisti, TipoOrdine, Frequenza, DataInizioCompetenza, DataFineCompetenza) " +
                    "VALUES (@guid, 987654321, 1, N'TEST', 1, 1, 15, N'Synthetic test request', GETDATE(), 1001, N'001', N'NB', 1, GETDATE(), GETDATE()); " +
                    "DECLARE @guidText varchar(40) = CONVERT(varchar(40), @guid); " +
                    "EXEC dbo.Flusso_RDA @pi_guid=@guidText, @po_valoreuscita=@output OUTPUT; " +
                    "SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    Assert.True(Convert.ToInt32(command.ExecuteScalar()) > 0);
                }
            }
        }

        [RequiresDatabaseFact]
        public void Remaining_sap_procedures_return_success_for_no_match_synthetic_inputs()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                foreach (var procedure in new[] { "ImportaBudget", "AggiornaFornitori", "AzzeraTargetSC00G100" })
                {
                    using (var command = new SqlCommand(
                        "DECLARE @output varchar(30); EXEC dbo." + procedure + " @pi_istituto=-32768, @po_valoreuscita=@output OUTPUT; SELECT @output;",
                        connection))
                    {
                        Assert.Equal(string.Empty, command.ExecuteScalar());
                    }
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_Ordini_SAP_ist_handles_no_matching_synthetic_requests()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "DECLARE @output varchar(40); EXEC dbo.Flusso_Ordini_SAP_ist @pi_istituto=-32768, @po_valoreuscita=@output OUTPUT; SELECT @output;",
                    connection))
                {
                    Assert.Equal(string.Empty, command.ExecuteScalar());
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_ist_handles_no_matching_synthetic_requests()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "DECLARE @output varchar(40); EXEC dbo.Flusso_RDA_ist @pi_istituto=-32768, @po_valoreuscita=@output OUTPUT; SELECT @output;",
                    connection))
                {
                    Assert.Equal(string.Empty, command.ExecuteScalar());
                }
            }
        }

        [RequiresDatabaseFact]
        public void Polling_ha_gli_oggetti_attesi()
        {
            Assert.Equal(9, Conteggio("Polling", "SELECT COUNT(*) FROM sys.tables"));
            Assert.Equal(1, Conteggio("Polling", "SELECT COUNT(*) FROM sys.views"));
            Assert.Equal(3, Conteggio("Polling", "SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF')"));
            Assert.Equal(8, Conteggio("Polling", "SELECT COUNT(*) FROM sys.procedures"));
        }

        [RequiresDatabaseFact]
        public void eProcurement_ha_gli_oggetti_recuperati_in_T0_12()
        {
            // Se questo test fallisce con tabelle=0, lo schema eProcurement non è stato
            // applicato: il deploy ne crea 15 più la vista e la funzione.
            Assert.Equal(15, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.tables"));
            Assert.Equal(1, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.views"));
            Assert.Equal(1, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF')"));
            Assert.Equal(5, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.foreign_keys"));
            Assert.Equal(22, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.default_constraints"));
            Assert.Equal(13, Conteggio("eProcurement", "SELECT COUNT(*) FROM sys.key_constraints WHERE type = 'PK'"));
        }

        [RequiresDatabaseFact]
        public void Quartz_ha_le_tabelle_del_job_store()
        {
            Assert.Equal(11, Conteggio("Quartz", "SELECT COUNT(*) FROM sys.tables WHERE name LIKE 'QRTZ[_]%'"));
        }

        [RequiresDatabaseFact]
        public void Le_due_tabelle_spostate_in_eProcurement_sono_li()
        {
            // T0.12 ha risolto l'ambiguità di collocazione (ADR 0001 §4, A2): appartengono a
            // eProcurement, non al database applicativo. Il test lo fissa.
            var inEproc = Nomi("eProcurement",
                "SELECT name FROM sys.tables WHERE name IN ('RDA_Esiti_LastOutput_OnLine','Tbl_Export_SAP_OnLine') ORDER BY name");
            Assert.Equal(2, inEproc.Count);

            var inPolling = Nomi("Polling",
                "SELECT name FROM sys.tables WHERE name IN ('RDA_Esiti_LastOutput_OnLine','Tbl_Export_SAP_OnLine')");
            Assert.Empty(inPolling);
        }

        [RequiresDatabaseFact]
        public void La_vista_applicativa_e_interrogabile()
        {
            // Unisce TaskService e DimProcessi: se la vista o una delle due tabelle manca,
            // la query fallisce.
            Conteggio("Polling", "SELECT COUNT(*) FROM dbo.ViewTaskService");
        }

        [RequiresDatabaseFact]
        public void Datini_e_Datfin_hanno_semantica_di_fine_e_inizio_giornata()
        {
            // I nomi sono fuorvianti: non sono "inizio mese" e "fine mese". Datini restituisce
            // la FINE della giornata, Datfin l'INIZIO. Scoperto in T0.7 leggendo il codice
            // durante la verifica del deploy. Vale la pena fissarlo: un futuro "riordino dei
            // nomi" non deve cambiare il comportamento delle procedure che li usano.
            const string sql =
                "SELECT CONVERT(varchar(23), dbo.Datini(DATEFROMPARTS(2024,3,15)), 121) + '|' + " +
                "       CONVERT(varchar(23), dbo.Datfin(DATEFROMPARTS(2024,3,15)), 121)";
            using (var conn = new SqlConnection(TestDatabase.ConnectionString()))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    Assert.Equal("2024-03-15 23:59:59.997|2024-03-15 00:00:00.000",
                        (string)cmd.ExecuteScalar());
                }
            }
        }

        [RequiresDatabaseFact]
        public void La_funzione_StripHTML_rimuove_i_tag()
        {
            // Recuperata da eProcurement in T0.12: il test conferma che non è solo presente,
            // ma funzionante.
            using (var conn = new SqlConnection(TestDatabase.ConnectionString("eProcurement")))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT dbo.StripHTML(N'<p>Ciao <b>mondo</b></p>')", conn))
                {
                    var risultato = (string)cmd.ExecuteScalar();
                    Assert.DoesNotContain("<", risultato);
                    Assert.Contains("Ciao", risultato);
                    Assert.Contains("mondo", risultato);
                }
            }
        }
    }
}
