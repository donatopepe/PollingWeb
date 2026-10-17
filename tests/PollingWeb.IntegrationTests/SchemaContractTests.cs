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
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        [RequiresDatabaseFact]
        public void Esiti_Ordini_SAP_marks_synthetic_successful_order_and_request()
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
                    "VALUES (N'4502987655',N'000',N'1000',N'001',NULL,N'Synthetic success'); " +
                    "EXEC dbo.Esiti_Ordini_SAP @po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RDA_Esiti WHERE Num_Ordine=N'4502987655' AND Cod_Errore IS NULL) THROW 51011,'Synthetic success was not archived',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdA WHERE GUID=@guid AND Status=20) THROW 51012,'Successful order status was not updated',1; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.RdE WHERE RdAID=@guid AND Status=20) THROW 51013,'Successful request status was not updated',1; " +
                    "ROLLBACK TRANSACTION; END TRY BEGIN CATCH IF @@TRANCOUNT>0 ROLLBACK TRANSACTION; THROW; END CATCH;",
                    connection))
                {
                    command.ExecuteNonQuery();
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
        public void Flusso_RDA_ist_executes_with_synthetic_request_and_item_inside_rollback()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @guid uniqueidentifier=NEWID(), @art uniqueidentifier=NEWID(), @output varchar(40); " +
                    "INSERT INTO eProcurement.dbo.RdA (GUID,Progressivo,PageID,Matricola,Istituto,IstID,Status,Oggetto,DataModifica,CFor,GruppoAcquisti,TipoOrdine,Frequenza,DataInizioCompetenza,DataFineCompetenza) " +
                    "VALUES (@guid,987654324,1,N'TEST',1,1,15,N'Synthetic request item',GETDATE(),1001,N'001',N'NB',1,GETDATE(),GETDATE()); " +
                    "INSERT INTO eProcurement.dbo.RDAArt (IDArt,RdAID,GUIDArt,GruppoMerci,DescrDett_Ord,QtaC) VALUES (987654324,@guid,@art,N'001',N'Synthetic item',1); " +
                    "INSERT INTO eProcurement.dbo.RDAArt_Costo_per_Anno (GUIDArt,DataInizioAnno,DataFineAnno,GGAnno,CostoAnno,AnnoCompetenza) VALUES (@art,GETDATE(),GETDATE(),1,10,2099); " +
                    "INSERT INTO eProcurement.dbo.DimCentridiResp (UoCode,IstID,CodiceColl) VALUES (N'TEST02',1,N'002'); " +
                    "UPDATE eProcurement.dbo.RdA SET CdRUOCode=N'TEST02' WHERE GUID=@guid; " +
                    "EXEC dbo.Flusso_RDA_ist @pi_istituto=1,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'RDA') THROW 51006,'Synthetic RDA institute flow produced no rows',1; " +
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
        public void AzzeraTargetSC00G100_deletes_only_requested_synthetic_institute()
        {
            using (var connection = new SqlConnection(TestDatabase.ConnectionString("Polling")))
            {
                connection.Open();
                using (var command = new SqlCommand(
                    "BEGIN TRANSACTION; BEGIN TRY " +
                    "DECLARE @output varchar(30); " +
                    "INSERT INTO dbo.TargetSC00G100 (Istituto,Cfor,Ragione_Sociale) VALUES (32002,987654321,N'Synthetic supplier'); " +
                    "EXEC dbo.AzzeraTargetSC00G100 @pi_istituto=32002,@po_valoreuscita=@output OUTPUT; " +
                    "IF EXISTS (SELECT 1 FROM dbo.TargetSC00G100 WHERE Istituto=32002) THROW 51004,'Synthetic institute was not cleared',1; " +
                    "IF @output<>'' THROW 51005,'Unexpected output value',1; " +
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
                    "VALUES (32001,987654321,N'Synthetic Supplier',N'Example Road',N'XY',N'00001',N'Test City',N'0000000000000001',N'00000000001'); " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32001,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID=32001 AND CFor=987654321 AND RagSoc=N'Synthetic Supplier') THROW 51002,'Synthetic supplier insert branch failed',1; " +
                    "UPDATE dbo.TargetSC00G100 SET Ragione_Sociale=N'Synthetic Supplier Updated' WHERE Istituto=32001 AND Cfor=987654321; " +
                    "EXEC dbo.AggiornaFornitori @pi_istituto=32001,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimFornitori WHERE IstID=32001 AND CFor=987654321 AND RagSoc=N'Synthetic Supplier Updated') THROW 51003,'Synthetic supplier update branch failed',1; " +
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
                    "EXEC dbo.ImportaBudget @pi_istituto=32000,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND BudgetAttuale=100 AND BudgetResiduo=70) THROW 51000,'Synthetic insert branch failed',1; " +
                    "UPDATE eProcurement.dbo.DimBudgetTemp SET BudgetRilasci=150,BudgetResiduo=120 WHERE Istituto=32000 AND Materiale=@material; " +
                    "EXEC dbo.ImportaBudget @pi_istituto=32000,@po_valoreuscita=@output OUTPUT; " +
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.DimBudget WHERE IstID=32000 AND SapID=@sapId AND Materiale=@material AND BudgetAttuale=150 AND BudgetResiduo=120 AND IsActive=1) THROW 51001,'Synthetic update branch failed',1; " +
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
                    "IF NOT EXISTS (SELECT 1 FROM eProcurement.dbo.Tbl_Export_SAP_OnLine WHERE Spid=@@SPID AND Tiporecord=N'ODA') THROW 51009,'Synthetic ODA institute flow produced no rows',1; " +
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
