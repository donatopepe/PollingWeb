-- =============================================================================
-- 03-permissions.sql — permessi minimi per il login applicativo
-- =============================================================================
-- Eseguire nel contesto del database di destinazione, dopo 02-create-app-user.sql.
-- Il solo GRANT CONNECT non basta: l'applicazione ha bisogno di EXECUTE sulle
-- stored procedure che invoca e di CRUD sulle tabelle applicative. L'assegnazione
-- puntuale dei permessi e' oggetto del task T0.8 (scripts/db-deploy.sh).
-- =============================================================================
GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GO
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;
GO

-- Accesso al database per l'utente applicativo.
GRANT CONNECT TO [$(PollingAppLogin)];
GO
