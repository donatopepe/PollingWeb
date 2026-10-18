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

        private static List<string> Nomi(string database, string sql, params SqlParameter[] parameters)
        {
            var risultato = new List<string>();
            using (var conn = new SqlConnection(TestDatabase.ConnectionString(database)))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(parameters);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            risultato.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return risultato;
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_executes_with_synthetic_error_result_and_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID, Progressivo, PageID, Matricola, IstID, Istituto, Status, NOrdSAP) " +
                    "VALUES (@guid, 987654323, 1, N'TEST', 1, 1, 15, 4502987654); " +
                    "INSERT INTO eProcurement.dbo.LogTime_Ciclo (Rch_ID, Rch_Progr, PageID, Matricola, StatoID, StatoIntID) " +
                    "VALUES (@guid, 1, 1, N'TEST', 15, 15); " +
                    "INSERT INTO eProcurement.dbo.RdE (GUID,Progressivo,PageID,IstID,RdAID,Status) VALUES (NEWID(),987654323,1,1,@guid,15); " +
                    "INSERT INTO eProcurement.dbo.RDA_Esiti_LastOutput_OnLine (Num_Ordine, Mandante, Societa, Posizione, Cod_Errore, Desc_Errore) " +
                    "VALUES (N'4502987654', N'000', N'1000', N'001', N'E001', N'Synthetic error'); " +
                    "EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine=N'4502987654' AND Cod_Errore=N'E001') THROW 51007,'Synthetic SAP error was not archived',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdA WHERE GUID=@guid AND Status=16) THROW 51008,'Synthetic request status was not updated',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID=@guid AND StatoID=20 AND StatoIntID=20) THROW 51037,'SAP error also produced a success transition',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID=@guid AND StatoID=16 AND StatoIntID=16)<>1 THROW 51032,'SAP error transition was not logged exactly once',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID=@guid AND StatoID=16 AND StatoIntID=16 AND Matricola=N'BBSISTE') THROW 51035,'SAP error log did not record the expected actor',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID=@guid AND StatoID=16 AND StatoIntID=16 AND Rch_Progr=1 AND PageID=1) THROW 51036,'SAP error log did not retain request context',1; " +
                    "IF @output<>'' THROW 51033,'SAP error result output was not empty',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine=N'4502987654' AND Cod_Errore=N'E001')<>1 THROW 51069,'Synthetic SAP error result was not archived exactly once',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_converts_synthetic_warning_to_success_and_archives_original()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,NOrdSAP) " +
                    "VALUES (@guid,987654327,1,N'TEST',1,1,15,4502987655); " +
                    "INSERT INTO eProcurement.dbo.LogTime_Ciclo (Rch_ID,Rch_Progr,PageID,Matricola,StatoID,StatoIntID) " +
                    "VALUES (@guid,1,1,N'TEST',15,15); " +
                    "INSERT INTO eProcurement.dbo.RdE (GUID,Progressivo,PageID,IstID,RdAID,Status) VALUES (NEWID(),987654327,1,1,@guid,15); " +
                    "INSERT INTO eProcurement.dbo.RDA_Esiti_LastOutput_OnLine (Num_Ordine,Mandante,Societa,Posizione,Cod_Errore,Desc_Errore) " +
                    "VALUES (N'4502987655',N'000',N'1000',N'001',N'W001',N'Synthetic warning'); " +
                    "INSERT INTO eProcurement.dbo.RDA_Esiti_LastOutput_OnLine (Num_Ordine,Mandante,Societa,Posizione,Cod_Errore,Desc_Errore) " +
                    "VALUES (N'4502987656',N'000',N'1000',N'002',N'E002',N'Synthetic error'); " +
                    "EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine=N'4502987655' AND Cod_Errore=N'W001') THROW 51011,'Original synthetic warning was not archived',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine WHERE Num_Ordine=N'4502987655' AND Cod_Errore IS NULL) THROW 51018,'Synthetic warning was not normalized',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdA WHERE GUID=@guid AND Status=20) THROW 51012,'Successful order status was not updated',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdE WHERE RdAID=@guid AND Status=20) THROW 51013,'Successful request status was not updated',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID=@guid AND StatoID=20 AND StatoIntID=20)<>1 THROW 51030,'Successful order transition was not logged exactly once',1; " +
                    "IF @output<>'' THROW 51057,'Warning-success result output was not empty',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine=N'4502987656' AND Cod_Errore=N'E002') THROW 51019,'Synthetic error result was not archived',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_success_remains_success_when_another_order_has_an_error()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,NOrdSAP) VALUES (@guid,987654351,1,N'TEST',1,1,15,4502987776); " +
                    "INSERT INTO eProcurement.dbo.RDA_Esiti_LastOutput_OnLine (Num_Ordine,Mandante,Societa,Posizione,Cod_Errore,Desc_Errore) VALUES " +
                    "(N'4502987776',N'000',N'1000',N'001',NULL,N'Synthetic success'),(N'4502988288',N'000',N'1000',N'002',N'E004',N'Other synthetic error'); " +
                    "EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdA WHERE GUID=@guid AND Status=20) THROW 51029,'Successful order was affected by an unrelated error',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_marks_multiple_synthetic_successful_orders_and_logs_transition()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guidA uniqueidentifier=NEWID(), @guidB uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,NOrdSAP) VALUES " +
                    "(@guidA,987654340,1,N'TEST',1,1,15,4502987660),(@guidB,987654341,1,N'TEST',1,1,15,4502987661); " +
                    "INSERT INTO eProcurement.dbo.RdE (GUID,Progressivo,PageID,IstID,RdAID,Status) VALUES (NEWID(),987654340,1,1,@guidA,15),(NEWID(),987654341,1,1,@guidB,15); " +
                    "INSERT INTO eProcurement.dbo.LogTime_Ciclo (Rch_ID,Rch_Progr,PageID,Matricola,StatoID,StatoIntID) VALUES (@guidA,1,1,N'TEST',15,15),(@guidB,2,1,N'TEST',15,15); " +
                    "INSERT INTO eProcurement.dbo.RDA_Esiti_LastOutput_OnLine (Num_Ordine,Mandante,Societa,Posizione,Cod_Errore,Desc_Errore) VALUES " +
                    "(N'4502987660',N'000',N'1000',N'001',NULL,N'Synthetic success A'),(N'4502987661',N'000',N'1000',N'001',NULL,N'Synthetic success B'); " +
                    "EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine IN (N'4502987660',N'4502987661') AND Cod_Errore IS NULL)<>2 THROW 51023,'Synthetic successes were not archived',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.RdA WHERE GUID IN (@guidA,@guidB) AND Status=20)<>2 THROW 51024,'Successful synthetic orders were not marked',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.RdE WHERE RdAID IN (@guidA,@guidB) AND Status=20)<>2 THROW 51025,'Successful synthetic requests were not marked',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.LogTime_Ciclo WHERE Rch_ID IN (@guidA,@guidB) AND StatoID=20 AND StatoIntID=20)<>2 THROW 51026,'Success transition log entries were not written',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine IN (N'4502987660',N'4502987661') AND Cod_Errore IS NULL)<>2 THROW 51075,'Repeated success processing duplicated archived results',1; " +
                    "IF @output<>'' THROW 51067,'Multiple-success Esiti_Ordini_SAP output was not empty',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Datini_and_Datfin_return_expected_synthetic_boundaries()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "SELECT dbo.Datini(CONVERT(datetime,'2096-02-29T12:34:56',126)), dbo.Datfin(CONVERT(datetime,'2096-02-29T12:34:56',126)), " +
                    "dbo.Datini(CAST(NULL AS datetime)), dbo.Datfin(CAST(NULL AS datetime)), " +
                    "dbo.Datini(CONVERT(datetime,'2000-01-01T23:59:59.997',126)), dbo.Datfin(CONVERT(datetime,'2000-01-01T23:59:59.997',126)), " +
                    "dbo.Datini(CONVERT(datetime,'2000-12-31T00:00:00',126)), dbo.Datfin(CONVERT(datetime,'2000-12-31T00:00:00',126))",
                    connection))
                using (var reader = command.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal(new DateTime(2096, 2, 29, 23, 59, 59, 997), reader.GetDateTime(0));
                    Assert.Equal(new DateTime(2096, 2, 29, 0, 0, 0), reader.GetDateTime(1));
                    Assert.True(reader.IsDBNull(2));
                    Assert.True(reader.IsDBNull(3));
                    Assert.Equal(new DateTime(2000, 1, 1, 23, 59, 59, 997), reader.GetDateTime(4));
                    Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), reader.GetDateTime(5));
                    Assert.Equal(new DateTime(2000, 12, 31, 23, 59, 59, 997), reader.GetDateTime(6));
                    Assert.Equal(new DateTime(2000, 12, 31, 0, 0, 0), reader.GetDateTime(7));
                }
            }
        }

        [RequiresDatabaseFact]
        public void StripHTML_functions_cover_empty_null_and_plain_text_inputs()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                foreach (var function in new[] { "dbo.StripHTML", "eProcurement.dbo.StripHTML" })
                {
                    using (var command = new SqlCommand(
                        "SELECT " + function + "(CAST(NULL AS nvarchar(max))), " +
                        function + "(N''), " + function + "(N'plain synthetic text'), " +
                        function + "(N'<p>synthetic, text-42</p>'), " +
                        function + "(N'left <b>middle'), " +
                        function + "(N'città, già'), " +
                        function + "(N'àèìòù'), " +
                        function + "(N'caffè'), " +
                        function + "(N'plain synthetic 123'), " +
                        function + "(N'quote''dash-tilde`apostrophe’'), " +
                        function + "(N'plain\r\nsynthetic'), " +
                        function + "(N'--'), " +
                        function + "(N'<b>alpha</b><i>beta</i>'), " +
                        function + "(N'left\tright'), " +
                        function + "(N'<></>'), " +
                        function + "(N'<b class=''synthetic''>alpha</b>'), " +
                        function + "(N'<b>outer <i>inner</i> end</b>'), " +
                        function + "(N'<B>alpha</B>'), " +
                        function + "(N'&#65;'), " +
                        function + "(N'left<br/>right'), " +
                        function + "(N'<b>   </b>'), " +
                        function + "(N'<b>left > right</b>'), " +
                        function + "(N'a,b'), " +
                        function + "(N'a,,b'), " +
                        function + "(N'x-y'), " +
                        function + "(N'a`b'), " +
                        function + "(N''''), " +
                        function + "(N'<b title=''a > b''>x</b>')",
                        connection))
                    using (var reader = command.ExecuteReader())
                    {
                        Assert.True(reader.Read());
                        Assert.True(reader.IsDBNull(0));
                        Assert.Equal("", reader.GetString(1));
                        Assert.Equal("plain synthetic text", reader.GetString(2));
                        Assert.Equal(" synthetic  text 42 ", reader.GetString(3));
                        Assert.Equal("left  middle", reader.GetString(4));
                        Assert.Equal("citta  gia", reader.GetString(5));
                        Assert.Equal("aeiou", reader.GetString(6));
                        Assert.Equal("caffe", reader.GetString(7));
                        Assert.Equal("plain synthetic 123", reader.GetString(8));
                        Assert.Equal("quote dash tilde apostrophe ", reader.GetString(9));
                        Assert.Equal("plain\r\nsynthetic", reader.GetString(10));
                        Assert.Equal("  ", reader.GetString(11));
                        Assert.Equal(" alpha  beta ", reader.GetString(12));
                        Assert.Equal("left\tright", reader.GetString(13));
                        Assert.Equal("  ", reader.GetString(14));
                        Assert.Equal(" alpha ", reader.GetString(15));
                        Assert.Equal(" outer  inner  end ", reader.GetString(16));
                        Assert.Equal(" alpha ", reader.GetString(17));
                        Assert.Equal("&#65;", reader.GetString(18));
                        Assert.Equal("left right", reader.GetString(19));
                        Assert.Equal("     ", reader.GetString(20));
                        Assert.Equal(" left > right ", reader.GetString(21));
                        Assert.Equal("a b", reader.GetString(22));
                        Assert.Equal("a  b", reader.GetString(23));
                        Assert.Equal("x y", reader.GetString(24));
                        Assert.Equal("a b", reader.GetString(25));
                        Assert.Equal(" ", reader.GetString(26));
                        Assert.Equal("  b >x ", reader.GetString(27));
                    }
                }
            }
        }

        [RequiresDatabaseFact]
        public void StripHTML_functions_return_expected_synthetic_text()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                foreach (var function in new[] { "dbo.StripHTML", "eProcurement.dbo.StripHTML" })
                {
                    using (var command = new SqlCommand("SELECT " + function + "(N'<b>synthetic-name</b>')", connection))
                    {
                        Assert.Equal(" synthetic name ", command.ExecuteScalar());
                    }
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_removes_previous_export_for_same_request_before_replacing_it()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @guidText varchar(40), @output varchar(40); " +
                    "SET @guidText=CONVERT(varchar(40),@guid); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (@guid,987654329,1,N'TEST',1,1,15,N'Synthetic request replacement',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE()); " +
                    "SET @guidText=CONVERT(varchar(40),@guid); " +
                    "INSERT INTO eProcurement.dbo.Tbl_Export_SAP_OnLine (Riga,Spid,Codist,Elaborato,Tiporecord,NumRda) " +
                    "VALUES (N'Synthetic stale request row',@@SPID,1,0,N'RDA',N'4502654329'); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST29',1,N'029'); " +
                    "EXEC dbo.Flusso_RDA @pi_guid=@guidText,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Riga=N'Synthetic stale request row') THROW 51046,'Stale synthetic request export was not replaced',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Codist=1 AND Tiporecord=N'RDA' AND NumRda=N'4502654329') THROW 51047,'Synthetic request export was not created',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_ist_replaces_stale_export_for_another_request_in_target_institute()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (NEWID(),987654328,1,N'TEST',1,1,15,N'Synthetic target institute request',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.Tbl_Export_SAP_OnLine (Riga,Spid,Codist,Elaborato,Tiporecord,NumRda) " +
                    "VALUES (N'Synthetic stale export from prior institute request',@@SPID,2,0,N'RDA',N'4502654328'); " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Riga=N'Synthetic stale export from prior institute request') THROW 51048,'Stale institute export was not removed',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Codist=1 AND Tiporecord=N'RDA') THROW 51049,'Institute request export was not generated',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_ist_clears_output_for_multiple_requests()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(40)='sentinel'; " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza,CdRUOCode) VALUES " +
                    "(NEWID(),987654335,1,N'TEST',1,1,15,N'Synthetic RDA output A',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE(),N'TEST35'),(NEWID(),987654336,1,N'TEST',1,1,15,N'Synthetic RDA output B',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE(),N'TEST35'); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST35',1,N'035'); " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>'' THROW 51081,'Multi-request institute RDA flow returned unexpected output',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Codist=1 AND Tiporecord=N'RDA')<2 THROW 51082,'Multi-request institute RDA flow produced too few rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
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
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode, IstID, CodiceColl) VALUES (N'TEST01', 1, N'001'); " +
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
        public void Flusso_RDA_ist_executes_synthetic_nb_request_and_item_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @art uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (@guid,987654329,1,N'TEST',1,1,15,N'Synthetic request NB',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,GUIDArt,GruppoMerci,DescrDett_Ord,QtaC,CostoConcordatoNoIva,DataInizioCompetenza,DataFineCompetenza) VALUES (987654329,@guid,@art,N'001',N'Synthetic NB item',1,100,GETDATE(),GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) VALUES (@art,GETDATE(),GETDATE(),1,100,2099); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST06',1,N'006'); " +
                    "UPDATE eProcurement.dbo.RdA SET CdRUOCode=N'TEST06' WHERE GUID=@guid; " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'RDA')<2 THROW 51020,'Synthetic RDA institute NB flow produced insufficient rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_RDA_ist_executes_synthetic_nc_request_and_item_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @art uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (@guid,987654324,1,N'TEST',1,1,15,N'Synthetic request item',GETDATE(),1001,N'001',N'NC',12,DATEFROMPARTS(2099,1,1),DATEFROMPARTS(2099,3,31)); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,GUIDArt,GruppoMerci,DescrDett_Ord,QtaC) VALUES (987654324,@guid,@art,N'001',N'Synthetic item',1); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) VALUES (@art,DATEFROMPARTS(2099,1,1),DATEFROMPARTS(2099,3,31),90,300,2099); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST02',1,N'002'); " +
                    "UPDATE eProcurement.dbo.RdA SET CdRUOCode=N'TEST02' WHERE GUID=@guid; " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'RDA')<2 THROW 51006,'Synthetic RDA institute NC flow produced insufficient rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_Ordini_SAP_exports_and_returns_empty_output()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(),@guidText varchar(40),@output varchar(40)='sentinel'; " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST34',1,N'034'); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza,CdRUOCode) " +
                    "VALUES (@guid,987654334,1,N'TEST',1,1,15,N'Synthetic ODA output',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE(),N'TEST34'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,SapID,GruppoMerci,DescrDett_Ord,QtaC,CostoConcordatoNoIva,DataPrevCons,TipoContabilizzazione) VALUES (987654334,@guid,1,N'001',N'Synthetic ODA output item',1,10,GETDATE(),N'Y'); " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP,Materiale) VALUES (1,N'SYNTHETIC'); " +
                    "INSERT INTO eProcurement.dbo.JoinCatSottoCat (CatID,SCID,SapID,CodPrg) VALUES (1,1,1,N'TEST'); " +
                    "SET @guidText=CONVERT(varchar(40),@guid); " +
                    "EXEC dbo.Flusso_Ordini_SAP @pi_guid=@guidText,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>'' THROW 51065,'Successful ODA flow returned unexpected output',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Codist=1 AND Tiporecord=N'ODA') THROW 51066,'Successful ODA flow did not insert an export row',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_Ordini_SAP_executes_with_synthetic_request_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier = NEWID(), @guidText varchar(40), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID, Progressivo, PageID, Matricola, IstID, Istituto, Status, " +
                    "Oggetto, DataModifica, CFor, GruppoAcquisti, TipoOrdine, Frequenza, DataInizioCompetenza, DataFineCompetenza) " +
                    "VALUES (@guid, 987654322, 1, N'TEST', 1, 1, 15, N'Synthetic order test', GETDATE(), 1001, N'001', N'NB', 1, GETDATE(), GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt, RdAID, SapID, QtaC, CostoConcordatoNoIva, DescrDett_Ord, DataPrevCons, TipoContabilizzazione) " +
                    "VALUES (987654322, @guid, 1, 1, 10, N'Synthetic item', GETDATE(), N'Y'); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode, IstID, CodiceColl) VALUES (N'TEST01', 1, N'001'); " +
                    "UPDATE eProcurement.dbo.RdA SET CdRUOCode=N'TEST01' WHERE GUID=@guid; " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP, Materiale) VALUES (1, N'SYNTHETIC'); " +
                    "INSERT INTO eProcurement.dbo.JoinCatSottoCat (CatID, SCID, SapID, CodPrg) VALUES (1, 1, 1, N'TEST'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) " +
                    "SELECT GUIDArt,GETDATE(),GETDATE(),1,10,2099 FROM eProcurement.dbo.RDAArt WHERE RdAID=@guid AND GUIDArt IS NOT NULL; " +
                    "UPDATE eProcurement.dbo.RDAArt SET GUIDArt=NEWID() WHERE RdAID=@guid AND GUIDArt IS NULL; " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) " +
                    "SELECT GUIDArt,GETDATE(),GETDATE(),1,10,2099 FROM eProcurement.dbo.RDAArt WHERE RdAID=@guid AND GUIDArt IS NOT NULL; " +
                    "SET @guidText = CONVERT(varchar(40), @guid); " +
                    "EXEC dbo.Flusso_Ordini_SAP @pi_guid=@guidText, @po_valoreuscita=@output OUTPUT; " +
                    "SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    Assert.True(Convert.ToInt32(command.ExecuteScalar()) > 0);
                }
            }
        }

        [RequiresDatabaseFact]
        public void AzzeraTargetSC00G100_clears_all_rows_for_target_institute_only()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES " +
                    "(32010,987654340,N'Synthetic target one'),(32010,987654341,N'Synthetic target two'),(32011,987654340,N'Synthetic neighboring institute'); " +
                    "EXEC dbo.AzzeraTargetSC00G100 @pi_istituto=32010,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32010) THROW 51060,'Target institute rows remained after clear',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32011 AND Cfor=987654340) THROW 51061,'Neighboring institute row was deleted',1; " +
                    "IF @output<>'' THROW 51062,'Target clear output was not empty',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AzzeraTargetSC00G100_empty_target_clears_output_without_deleting_other_rows()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30)='sentinel'; " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES (32008,987654322,N'Synthetic unaffected institute'); " +
                    "EXEC dbo.AzzeraTargetSC00G100 @pi_istituto=32009,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>'' THROW 51058,'Empty-target clear returned unexpected output',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32008 AND Cfor=987654322) THROW 51059,'Empty-target clear deleted an unrelated institute row',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AzzeraTargetSC00G100_deletes_only_requested_synthetic_institute()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES " +
                    "(32002,987654321,N'Synthetic requested institute'),(32003,987654321,N'Synthetic other institute'); " +
                    "EXEC dbo.AzzeraTargetSC00G100 @pi_istituto=32002,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32002) THROW 51004,'Synthetic institute was not cleared',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32003 AND Cfor=987654321) THROW 51005,'Synthetic row for another institute was deleted',1; " +
                    "IF @output<>'' THROW 51006,'Unexpected output value',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AggiornaFornitori_updates_multiple_synthetic_suppliers_for_one_institute()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES (32003,987654330,N'Synthetic supplier A'),(32003,987654331,N'Synthetic supplier B'); " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32003,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimFornitori WHERE IstID=32003 AND CFor IN (987654330,987654331))<>2 THROW 51021,'Synthetic bulk insert failed',1; " +
                    "UPDATE dbo.TargetSC00G100 SET Ragione_Sociale=Ragione_Sociale+N' updated' WHERE Istituto=32003; " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32003,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimFornitori WHERE IstID=32003 AND RagSoc LIKE N'% updated')<>2 THROW 51022,'Synthetic bulk update failed',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimFornitori WHERE IstID=32003 AND CFor IN (987654330,987654331) AND RagSoc LIKE N'% updated')<>2 THROW 51072,'Bulk update did not preserve supplier identities',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID=32003 AND CFor IN (987654330,987654331) AND (Indirizzo IS NOT NULL OR Cap IS NOT NULL OR Citta IS NOT NULL OR Prov IS NOT NULL OR CodFisc IS NOT NULL OR PIva IS NOT NULL)) THROW 51080,'Bulk refresh did not propagate nullable source fields',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID<>32003 AND IstID IS NOT NULL AND RagSoc LIKE N'% updated') THROW 51023,'Bulk supplier update escaped institute scope',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AggiornaFornitori_empty_target_does_not_affect_nearby_institute_with_same_supplier()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES (32013,987654350,N'Synthetic neighboring supplier'); " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32014,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32013 AND Cfor=987654350 AND Ragione_Sociale=N'Synthetic neighboring supplier') THROW 51077,'Empty-target supplier update touched neighboring institute source row',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID=32014) THROW 51078,'Empty-target supplier update created an unexpected supplier',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AggiornaFornitori_empty_target_clears_output_without_touching_rows()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30)='sentinel'; " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32007,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>N'' THROW 51055,'Empty-target supplier refresh returned unexpected output',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID=32007) THROW 51056,'Empty-target supplier refresh unexpectedly created suppliers',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void AggiornaFornitori_inserts_then_updates_synthetic_supplier_and_rolls_back()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale,Indirizzo,Prov,Cap,Citta,Codfis,Piva) " +
                    "VALUES (32001,987654321,N'Synthetic Supplier',N'Example Road',N'XY',N'00001',N'Test City',N'0000000000000001',N'00000000001'), " +
                    "(32001,987654332,N'Synthetic Supplier Two',N'Example Road 2',N'XZ',N'00002',N'Test City 2',N'0000000000000002',N'00000000002'); " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32001,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimFornitori WHERE IstID=32001 AND CFor IN (987654321,987654332))<>2 THROW 51002,'Synthetic supplier bulk insert branch failed',1; " +
                    "UPDATE dbo.TargetSC00G100 SET Ragione_Sociale=Ragione_Sociale+N' Updated' WHERE Istituto=32001; " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32001,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimFornitori WHERE IstID=32001 AND CFor IN (987654321,987654332) AND RagSoc LIKE N'%Updated')<>2 THROW 51003,'Synthetic supplier bulk update branch failed',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_inserts_then_updates_synthetic_budget_and_rolls_back()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @material nvarchar(10)=N'T' + RIGHT(REPLACE(CONVERT(nvarchar(36),NEWID()),N'-',N''),9), " +
                    "@sapId int=(SELECT ISNULL(MAX(IDCatSAP),0)+1000 FROM eProcurement.dbo.DimSottoCatSAP), " +
                    "@date datetime=DATEFROMPARTS(2099,12,31), @output varchar(30); " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP,Materiale) VALUES (@sapId,@material); " +
                    "INSERT INTO eProcurement.dbo.DimBudgetTemp (Istituto,CodCDR,Materiale,BudgetRilasci,BudgetResiduo,CodFondo,Datarif) " +
                    "VALUES (32000,N'TEST',@material,100,70,N'SYNTHETIC',@date); " +
                    "INSERT INTO eProcurement.dbo.DimBudget (IstID,CodFondo,SapID,Materiale,CodCdR,BudgetAttuale,BudgetResiduo,DataRif,IsActive) " +
                    "VALUES (32000,N'SYNTHETIC',@sapId,@material,N'TEST',1,2,DATEFROMPARTS(2098,12,31),1); " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32000,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND BudgetAttuale=100 AND BudgetResiduo=70 AND IsActive=1) THROW 51000,'Synthetic insert branch failed',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND DataRif=DATEFROMPARTS(2098,12,31) AND IsActive=0) THROW 51016,'Existing budget was not deactivated',1; " +
                    "UPDATE eProcurement.dbo.DimBudgetTemp SET BudgetRilasci=150,BudgetResiduo=120 WHERE Istituto=32000 AND Materiale=@material; " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32000,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND BudgetAttuale=150 AND BudgetResiduo=120 AND IsActive=1) THROW 51001,'Synthetic update branch failed',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND IsActive=1 AND (BudgetAttuale<>150 OR BudgetResiduo<>120)) THROW 51074,'Budget update left an older active value for the same key',1; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material)<>2 THROW 51073,'Budget update created duplicate matching records',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID<>32000 AND IsActive=1) THROW 51038,'ImportaBudget deactivated budgets from another institute',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND (SapID<>@sapId OR Materiale<>@material) AND IsActive=0 AND DataRif=DATEFROMPARTS(2098,12,31)) THROW 51039,'ImportaBudget deactivated an unrelated institute budget',1; " +
                    "DELETE FROM eProcurement.dbo.DimBudgetTemp WHERE Istituto=32000 AND Materiale=@material; " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32000,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND IsActive=0) THROW 51017,'Removed budget was not deactivated',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_no_match_institute_deactivates_only_its_existing_budgets()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO eProcurement.dbo.DimBudget (IstID,CodFondo,SapID,Materiale,CodCdR,BudgetAttuale,BudgetResiduo,DataRif,IsActive) " +
                    "VALUES (32003,N'SYNTHETIC',999998,N'NO-MATCH-A',N'TEST',75,55,DATEFROMPARTS(2098,12,30),1),(32004,N'SYNTHETIC',999997,N'NO-MATCH-B',N'TEST',85,65,DATEFROMPARTS(2098,12,30),1); " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32003,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32003 AND SapID=999998 AND IsActive=1) THROW 51044,'ImportaBudget did not deactivate the target institute budget',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32004 AND SapID=999997 AND IsActive=1 AND BudgetAttuale=85 AND BudgetResiduo=65) THROW 51045,'ImportaBudget changed another institute budget',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_no_match_institute_deactivates_multiple_active_budgets()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO eProcurement.dbo.DimBudget (IstID,CodFondo,SapID,Materiale,CodCdR,BudgetAttuale,BudgetResiduo,DataRif,IsActive) VALUES " +
                    "(32012,N'SYNTHETIC',999994,N'NO-MATCH-D',N'TEST',10,5,DATEFROMPARTS(2098,12,28),1),(32012,N'SYNTHETIC',999995,N'NO-MATCH-E',N'TEST',20,15,DATEFROMPARTS(2098,12,27),1); " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32012,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.DimBudget WHERE IstID=32012 AND IsActive=0 AND SapID IN (999994,999995))<>2 THROW 51070,'No-match ImportaBudget did not deactivate every active target budget',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32012 AND IsActive=1) THROW 51071,'No-match ImportaBudget left an active target budget',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_no_match_institute_empty_rows_returns_empty_output()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30)='unexpected'; " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32006,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>N'' THROW 51054,'Empty-row ImportaBudget returned unexpected output',1; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32006) THROW 51079,'Empty-row ImportaBudget created unexpected budget rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_no_match_institute_preserves_budget_amounts_while_deactivating()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO eProcurement.dbo.DimBudget (IstID,CodFondo,SapID,Materiale,CodCdR,BudgetAttuale,BudgetResiduo,DataRif,IsActive) " +
                    "VALUES (32005,N'SYNTHETIC',999996,N'NO-MATCH-C',N'TEST',175,125,DATEFROMPARTS(2098,12,29),1); " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32005,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32005 AND SapID=999996 AND IsActive=0 AND BudgetAttuale=175 AND BudgetResiduo=125 AND DataRif=DATEFROMPARTS(2098,12,29)) THROW 51053,'No-match ImportaBudget failed to deactivate while preserving existing budget values',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void ImportaBudget_no_match_institute_leaves_existing_synthetic_budgets_unchanged()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO eProcurement.dbo.DimBudget (IstID,CodFondo,SapID,Materiale,CodCdR,BudgetAttuale,BudgetResiduo,DataRif,IsActive) " +
                    "VALUES (32002,N'SYNTHETIC',999999,N'NO-MATCH',N'TEST',75,55,DATEFROMPARTS(2098,12,30),1); " +
                    "EXEC dbo.ImportaBudget @pi_istituto=-32768,@po_valoreuscita=@output OUTPUT; " +
                    "IF @output<>N'' THROW 51042,'No-match ImportaBudget returned unexpected output',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32002 AND SapID=999999 AND Materiale=N'NO-MATCH' AND IsActive=1 AND BudgetAttuale=75 AND BudgetResiduo=55) THROW 51043,'No-match ImportaBudget changed an unrelated synthetic budget',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
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
        public void Flusso_Ordini_SAP_ist_executes_with_synthetic_nb_item_and_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza,CdRUOCode) " +
                    "VALUES (@guid,987654325,1,N'TEST',1,1,15,N'Synthetic NB test',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE(),N'TEST03'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,SapID,GruppoMerci,DescrDett_Ord,QtaC,CostoConcordatoNoIva,DataPrevCons,TipoContabilizzazione) " +
                    "VALUES (987654325,@guid,1,N'001',N'Synthetic NB item',2,20,GETDATE(),N'Y'); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST03',1,N'003'); " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP,Materiale) VALUES (1,N'SYNTHETIC'); " +
                    "INSERT INTO eProcurement.dbo.JoinCatSottoCat (CatID,SCID,SapID,CodPrg) VALUES (1,1,1,N'TEST'); " +
                    "EXEC dbo.Flusso_Ordini_SAP_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_Ordini_SAP_ist_executes_with_synthetic_nb_item_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza,CdRUOCode) " +
                    "VALUES (@guid,987654326,1,N'TEST',47,47,15,N'Synthetic institute order',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE(),N'TEST04'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,SapID,GruppoMerci,DescrDett_Ord,QtaC,CostoConcordatoNoIva,DataPrevCons,TipoContabilizzazione,GUIDArt) " +
                    "VALUES (987654326,@guid,1,N'001',N'Synthetic institute item',2,20,GETDATE(),N'Y',NEWID()); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST04',47,N'004'); " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP,Materiale) VALUES (1,N'SYNTHETIC'); " +
                    "INSERT INTO eProcurement.dbo.JoinCatSottoCat (CatID,SCID,SapID,CodPrg) VALUES (1,1,1,N'TEST'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) " +
                    "SELECT GUIDArt,GETDATE(),GETDATE(),1,20,2099 FROM eProcurement.dbo.RDAArt WHERE RdAID=@guid; " +
                    "EXEC dbo.Flusso_Ordini_SAP_ist @pi_istituto=47,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'ODA') THROW 51010,'Synthetic ODA institute item flow produced no rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Flusso_Ordini_SAP_ist_executes_synthetic_nc_monthly_item_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza,CdRUOCode) " +
                    "VALUES (@guid,987654328,1,N'TEST',46,46,15,N'Synthetic NC monthly',GETDATE(),1001,N'001',N'NC',12,DATEFROMPARTS(2099,1,1),DATEFROMPARTS(2099,3,31),N'TEST05'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,SapID,GruppoMerci,DescrDett_Ord,QtaC,CostoConcordatoNoIva,DataInizioCompetenza,DataFineCompetenza,TipoContabilizzazione,GUIDArt) " +
                    "VALUES (987654328,@guid,1,N'001',N'Synthetic NC item',1,300,DATEFROMPARTS(2099,1,1),DATEFROMPARTS(2099,3,31),N'Y',NEWID()); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST05',46,N'005'); " +
                    "INSERT INTO eProcurement.dbo.DimSottoCatSAP (IDCatSAP,Materiale) VALUES (1,N'SYNTHETIC'); " +
                    "INSERT INTO eProcurement.dbo.JoinCatSottoCat (CatID,SCID,SapID,CodPrg) VALUES (1,1,1,N'TEST'); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) " +
                    "SELECT GUIDArt,DataInizioCompetenza,DataFineCompetenza,90,300,2099 FROM eProcurement.dbo.RDAArt WHERE RdAID=@guid; " +
                    "EXEC dbo.Flusso_Ordini_SAP_ist @pi_istituto=46,@po_valoreuscita=@output OUTPUT; " +
                    "IF (SELECT COUNT(*) FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'ODA')<2 THROW 51015,'Synthetic NC monthly flow produced insufficient rows',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
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
        public void Flusso_RDA_ist_replaces_stale_export_for_same_order_across_institutes()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,IstID,Istituto,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (NEWID(),987654326,1,N'TEST',1,1,15,N'Synthetic target order',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.Tbl_Export_SAP_OnLine (Riga,Spid,Codist,Elaborato,Tiporecord,NumRda) " +
                    "VALUES (N'Synthetic stale export for matching order number',@@SPID,2,0,N'RDA',N'4502654326'); " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Riga=N'Synthetic stale export for matching order number') THROW 51051,'Matching synthetic stale export was not removed',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Codist=1 AND Tiporecord=N'RDA') THROW 51052,'Target institute export was not generated',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
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
        public void Datini_Datfin_cover_leap_day_and_year_boundary_dates()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "SELECT CONVERT(varchar(23),dbo.Datini(DATEFROMPARTS(2024,2,29)),121), " +
                    "CONVERT(varchar(23),dbo.Datfin(DATEFROMPARTS(2024,2,29)),121), " +
                    "CONVERT(varchar(23),dbo.Datini(DATEFROMPARTS(2024,12,31)),121), " +
                    "CONVERT(varchar(23),dbo.Datfin(DATEFROMPARTS(2025,1,1)),121)",
                    connection))
                using (var reader = command.ExecuteReader())
                {
                    Assert.True(reader.Read());
                    Assert.Equal("2024-02-29 23:59:59.997", reader.GetString(0));
                    Assert.Equal("2024-02-29 00:00:00.000", reader.GetString(1));
                    Assert.Equal("2024-12-31 23:59:59.997", reader.GetString(2));
                    Assert.Equal("2025-01-01 00:00:00.000", reader.GetString(3));
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
