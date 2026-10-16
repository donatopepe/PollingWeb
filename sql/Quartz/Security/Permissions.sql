-- =============================================================================
-- Permissions.sql — permessi a privilegi minimi per il login applicativo
-- =============================================================================
-- Database: Quartz
-- Eseguire dopo 02-create-app-user.sql e dopo aver applicato lo schema.
-- Le variabili sono fornite da scripts/db-deploy.sh (--with-secure-login).
--
-- Principio: l'utente applicativo NON e' db_owner, NON ha ruoli server e NON ha
-- alcun permesso DDL. Riceve esattamente:
--   tabelle   : SELECT, INSERT, UPDATE, DELETE   (11 oggetti)
--   viste     : SELECT                            (0 oggetti)
--   funzioni  : EXECUTE                          (0 oggetti)
--   procedure : EXECUTE                           (0 oggetti)
---- Nota: Quartz.NET con job store ADO esegue SQL diretto sulle tabelle
-- QRTZ_*, quindi servono tutti e quattro i verbi. Non usa procedure.

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

GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_BLOB_TRIGGERS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_CALENDARS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_CRON_TRIGGERS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_FIRED_TRIGGERS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_JOB_DETAILS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_LOCKS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_SCHEDULER_STATE] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_SIMPLE_TRIGGERS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_SIMPROP_TRIGGERS] TO [$(PollingAppLogin)];
GRANT SELECT, INSERT, UPDATE, DELETE ON [dbo].[QRTZ_TRIGGERS] TO [$(PollingAppLogin)];
GO
