-- =============================================================================
-- 02-create-app-user.sql — utente a livello database per il login applicativo
-- =============================================================================
-- Eseguire nel contesto del database di destinazione (Polling, eProcurement,
-- Quartz) dopo 01-create-app-login.sql.
--
-- Esecuzione manuale:
--   sqlcmd -S <server> -d Polling -v PollingAppLogin="PollingApp" -i 02-create-app-user.sql
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(PollingAppLogin)')
BEGIN
    CREATE USER [$(PollingAppLogin)] FOR LOGIN [$(PollingAppLogin)];
END
GO
