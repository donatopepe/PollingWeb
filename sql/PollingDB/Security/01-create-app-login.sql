-- =============================================================================
-- 01-create-app-login.sql — login applicativo a privilegi minimi
-- =============================================================================
-- Nessuna password e' memorizzata nel repository: nome e password arrivano da
-- variabili sqlcmd, con default definiti in PollingDB.sqlproj.
--
-- Esecuzione manuale:
--   sqlcmd -S <server> -d Polling \
--          -v PollingAppLogin="PollingApp" \
--             PollingAppPassword="<password-forte>" \
--          -i 01-create-app-login.sql
--
-- Hardening applicato rispetto alla versione storica:
--   * CHECK_POLICY = ON   (prima OFF: nessuna policy di scadenza/complessita')
--   * SID esplicito rimosso (prima il login era replicabile su qualsiasi istanza)
--   * password non piu' hardcoded
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(PollingAppLogin)')
BEGIN
    CREATE LOGIN [$(PollingAppLogin)]
        WITH PASSWORD = N'$(PollingAppPassword)',
             DEFAULT_LANGUAGE = [Italiano],
             CHECK_POLICY = ON;
END
GO
