# deploy/ — come applicare lo schema

Il percorso supportato è [`scripts/db-deploy.sh`](../../scripts/db-deploy.sh).
Questo documento spiega **l'ordine** e **perché** quell'ordine.

## Ordine di applicazione

| # | Passo | Destinazione | Note |
|---|-------|--------------|------|
| 1 | creazione dei database | istanza | `Polling`, `eProcurement`, `Quartz` |
| 2 | `PollingDB/dbo/Tables/*.sql` | `Polling` | nessuna dipendenza esterna |
| 3 | `PollingDB/dbo/Functions/*.sql` | `Polling` | indipendenti |
| 4 | `PollingDB/dbo/Views/*.sql` | `Polling` | dipendono dalle tabelle |
| 5 | `PollingDB/dbo/Stored Procedures/*.sql` | `Polling` | dipendono da tabelle e viste |
| 6 | `Quartz/quartz-schema.sql` | `Quartz` | indipendente |
| 7 | `PollingDB/Security/*.sql` | istanza + ogni database | richiede `POLLING_APP_PASSWORD` |
| 8 | `samples/seed/seed.sql` | `Polling` | facoltativo (`--with-seed`), dati **sintetici** |

**Perché le stored procedure vanno per ultime.** SQL Server applica la *name resolution
differita*: `CREATE PROCEDURE` non valida gli oggetti referenziati. Le procedure verrebbero
create anche su un database vuoto, con l'errore che emerge solo alla prima esecuzione. Per
tabelle e viste invece l'ordine è obbligatorio, perché i riferimenti sono validati alla
creazione.

**Perché le funzioni precedono le viste.** Le viste possono richiamare funzioni scalari; le
funzioni no.

## Modalità

| Modalità | Effetto | Quando usarla |
|----------|---------|---------------|
| `recreate` (default) | elimina e ricrea i tre database, poi applica tutto | sviluppo e test. Deterministico e **idempotente**: due esecuzioni danno lo stesso risultato |
| `apply` | crea i database se assenti e applica lo schema senza eliminarli | inizializzare un'istanza nuova. **Non** idempotente sugli oggetti già presenti |

`recreate` è **distruttivo**: cancella i dati nei tre database. Non usarlo su un'istanza con
dati che interessano.

## Migrazioni incrementali: problema aperto

Gli script sono `CREATE TABLE`, `CREATE VIEW`, `CREATE FUNCTION`, `CREATE PROCEDURE` — non
`CREATE OR ALTER`. Applicarli a un database che contiene già quegli oggetti **fallisce**.

Non esiste ancora una strategia di migrazione. Le opzioni, da valutare quando il deploy
cross-platform sarà consolidato:

1. **script numerati** applicati una volta sola, con una tabella di stato delle migrazioni;
2. **`CREATE OR ALTER`** per procedure, viste e funzioni, e script di `ALTER TABLE` per le
   modifiche strutturali;
3. **DACPAC** generato da SSDT: richiede Windows in fase di generazione, ma il deploy è
   cross-platform. È l'opzione più solida e la meno economica da introdurre.

Fino ad allora vale la regola: **il deploy è per istanza nuova**.

## Verifica

```bash
# deploy completo, distruttivo, con dati sintetici
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed

# seconda esecuzione: deve dare lo stesso risultato (idempotenza)
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed
```

Controlli sull'esito, verificati il 2026-10-16 su SQL Server 2022 in container:

```sql
-- Polling: 11 tabelle, 1 vista, 3 funzioni, 8 procedure
SELECT COUNT(*) FROM sys.tables;
SELECT COUNT(*) FROM sys.views;
SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF');
SELECT COUNT(*) FROM sys.procedures;

-- Quartz: 11 tabelle
SELECT COUNT(*) FROM sys.tables WHERE name LIKE 'QRTZ[_]%';

-- vista funzionante (unisce TaskService e DimProcessi)
SELECT COUNT(*) FROM ViewTaskService;      -- 3 con i dati di samples/
```
