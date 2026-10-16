-- =============================================================================
-- Permissions.sql — permessi a privilegi minimi per il login applicativo
-- =============================================================================
-- Database: eProcurement
-- Eseguire dopo 02-create-app-user.sql e dopo aver applicato lo schema.
-- Le variabili sono fornite da scripts/db-deploy.sh (--with-secure-login).
--
-- Principio: l'utente applicativo NON e' db_owner, NON ha ruoli server e NON ha
-- alcun permesso DDL. Riceve esattamente:
--   tabelle   : SELECT, INSERT, UPDATE, DELETE   (15 oggetti)
--   viste     : SELECT                            (1 oggetti)
--   funzioni  : EXECUTE                          (1 oggetti)
--   procedure : EXECUTE                           (0 oggetti)
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

GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimBudget] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimBudgetTemp] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimCentridiResp] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimFornitori] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[DimSottoCatSAP] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[JoinCatSottoCat] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[LogTime_Ciclo] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Log_ODA_SAP] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RDAArt] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RDAArt_Costo_per_Anno] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RDA_Esiti] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RDA_Esiti_LastOutput_OnLine] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RdA] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[RdE] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[Tbl_Export_SAP_OnLine] TO [$(PollingAppLogin)];
GO

-- Viste: sola lettura (SELECT, non EXECUTE: una vista non e' eseguibile).

GRANT SELECT ON [dbo].[ViewEstrazioneRdARdE] TO [$(PollingAppLogin)];
GO

-- Funzioni scalari: EXECUTE.
-- Verificato empiricamente: GRANT SELECT su una funzione scalare e'
-- rifiutato con Msg 4606 ('privilege SELECT is not compatible with
-- object'). Per le funzioni con valori di tabella servirebbe SELECT.

GRANT EXECUTE ON [dbo].[StripHTML] TO [$(PollingAppLogin)];
GO
