-- =============================================================================
-- Permissions.sql — permessi a privilegi minimi per il login applicativo
-- =============================================================================
-- Database: Polling
-- Eseguire dopo 02-create-app-user.sql e dopo aver applicato lo schema.
-- Le variabili sono fornite da scripts/db-deploy.sh (--with-secure-login).
--
-- Principio: l'utente applicativo NON e' db_owner, NON ha ruoli server e NON ha
-- alcun permesso DDL. Riceve esattamente:
--   tabelle   : SELECT, INSERT, UPDATE, DELETE   (9 oggetti)
--   viste     : SELECT                            (1 oggetti)
--   funzioni  : EXECUTE                          (3 oggetti)
--   procedure : EXECUTE                           (8 oggetti)
--
-- I permessi sono elencati oggetto per oggetto, non con GRANT sul livello schema:
-- cosi' l'intento e' verificabile leggendo il file, e un oggetto aggiunto senza
-- permesso e' un errore visibile invece di un permesso implicito non voluto.
-- scripts/db-deploy.sh esegue un auto-test che fallisce se un oggetto dello schema
-- non e' accessibile all'utente applicativo: la manutenzione di questo file e'
-- quindi verificata a ogni deploy, non affidata alla memoria.
--
-- Non sono piu' presenti i GRANT ... TO PUBLIC generati da SSDT sulle chiavi di
-- crittografia: non servono all'applicazione e allargavano i permessi.
-- =============================================================================

-- Accesso al database.
GRANT CONNECT TO [$(PollingAppLogin)];
GO


-- Tabelle: lettura e scrittura.

GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimDayquartz] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimProcessi] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimStato] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[ListaTriggerTemp] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[LogProcedure] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[LogTaskService] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[TargetSC00G100] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[TaskService] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Tool] TO [$(PollingAppLogin)];
GO

-- Viste: sola lettura (SELECT, non EXECUTE: una vista non e' eseguibile).

GRANT SELECT ON [dbo].[ViewTaskService] TO [$(PollingAppLogin)];
GO

-- Funzioni scalari: EXECUTE.
-- Verificato empiricamente: GRANT SELECT su una funzione scalare e'
-- rifiutato con Msg 4606 ('privilege SELECT is not compatible with
-- object'). Per le funzioni con valori di tabella servirebbe SELECT.

GRANT EXECUTE ON [dbo].[Datfin] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Datini] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[StripHTML] TO [$(PollingAppLogin)];
GO

-- Stored procedure: sola esecuzione.

GRANT EXECUTE ON [dbo].[AggiornaFornitori] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[AzzeraTargetSC00G100] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Esiti_Ordini_SAP] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Flusso_Ordini_SAP] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Flusso_Ordini_SAP_ist] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Flusso_RDA] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[Flusso_RDA_ist] TO [$(PollingAppLogin)];
GRANT EXECUTE ON [dbo].[ImportaBudget] TO [$(PollingAppLogin)];
GO
