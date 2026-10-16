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
| 6 | `eProcurement/dbo/Tables/*.sql` | `eProcurement` | lo schema acquisti, recuperato in sola DDL |
| 7 | `eProcurement/dbo/Functions|Views/*.sql` | `eProcurement` | funzioni prima delle viste |
| 8 | `Quartz/quartz-schema.sql` | `Quartz` | indipendente |
| 9 | `01-create-app-login.sql` | istanza | login a livello server, `CHECK_POLICY = ON` |
| 10 | `02-create-app-user.sql` + `Permissions.sql` | ogni database | utente + permessi a privilegi minimi |
| 11 | **autotest dei permessi** | tutti | fallisce se un permesso manca **o se ne esiste uno di troppo** |
| 12 | `samples/seed/seed.sql` | `Polling` | facoltativo (`--with-seed`), dati **sintetici** |

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

## Permessi a privilegi minimi

L'utente applicativo viene creato con `POLLING_APP_PASSWORD` e riceve permessi
**oggetto per oggetto**, definiti in tre file generati:

```
sql/PollingDB/Security/Permissions.sql        9 tabelle · 1 vista · 3 funzioni · 8 procedure
sql/eProcurement/Security/Permissions.sql    15 tabelle · 1 vista · 1 funzione
sql/Quartz/Security/Permissions.sql          11 tabelle
```

| Tipo di oggetto | Permesso |
|-----------------|----------|
| tabelle | `SELECT`, `INSERT`, `UPDATE`, `DELETE` |
| viste | `SELECT` |
| funzioni scalari | `EXECUTE` |
| stored procedure | `EXECUTE` |

Non riceve: `db_owner`, ruoli server, permessi DDL, accesso in scrittura ai database di
sistema. Verificato a ogni deploy.

### Due dettagli non ovvi, verificati empiricamente

1. **Le funzioni scalari richiedono `EXECUTE`, non `SELECT`.**
   `GRANT SELECT ON [dbo].[Datini]` viene rifiutato con
   `Msg 4606: Granted or revoked privilege SELECT is not compatible with object`.
   Per le funzioni con valori di tabella servirebbe invece `SELECT`: se in futuro ne
   vengono aggiunte, il generatore dei permessi va aggiornato di conseguenza.
2. **Le viste richiedono `SELECT`, non `EXECUTE`.** Associazione inversa, stessa classe di
   errore.

I permessi sono elencati oggetto per oggetto invece che con `GRANT` sul livello schema:
`GRANT` sullo schema si estenderebbe automaticamente anche agli oggetti futuri, mentre
l'elenco puntuale rende l'intento verificabile leggendo il file. Il rischio opposto — un
oggetto aggiunto senza permesso — è coperto da due controlli automatici:

- l'**autotest** del passo 11 esegue, come utente applicativo, letture, scritture, una
  `EXECUTE` di procedura, una funzione e accessi cross-database; e verifica che il DDL sia
  **negato** su tutti e tre i database e in scrittura su `master`;
- la **verifica di drift** seguente conta gli oggetti senza alcun permesso per l'utente:

```sql
SELECT o.name FROM sys.objects o
WHERE o.schema_id = SCHEMA_ID('dbo')
  AND o.type IN ('U','V','P','FN','IF','TF')
  AND NOT EXISTS (SELECT 1 FROM sys.database_permissions dp
                  WHERE dp.major_id = o.object_id
                    AND dp.grantee_principal_id = DATABASE_PRINCIPAL_ID('PollingApp'));
-- deve restituire zero righe
```

### Rigenerare i permessi dopo aver aggiunto un oggetto

I tre file sono generati dallo schema deployato, non scritti a mano: 38 oggetti trascritti
a mano sono 38 occasioni di errore. Dopo aver aggiunto un oggetto allo schema, rigenerarli
e rileggere il diff prima di committare.

## Verifica

```bash
# deploy completo, distruttivo, con dati sintetici
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed

# seconda esecuzione: deve dare lo stesso risultato (idempotenza)
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed
```

Controlli sull'esito, verificati il 2026-10-16 su SQL Server 2022 in container:

```sql
-- Polling: 9 tabelle, 1 vista, 3 funzioni, 8 procedure
SELECT COUNT(*) FROM sys.tables;
SELECT COUNT(*) FROM sys.views;
SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN','IF','TF');
SELECT COUNT(*) FROM sys.procedures;

-- eProcurement: 15 tabelle, 1 vista, 1 funzione, 5 FK, 22 default, 13 PK
SELECT COUNT(*) FROM sys.tables;
SELECT COUNT(*) FROM sys.foreign_keys;

-- Quartz: 11 tabelle
SELECT COUNT(*) FROM sys.tables WHERE name LIKE 'QRTZ[_]%';

-- vista funzionante (unisce TaskService e DimProcessi)
SELECT COUNT(*) FROM ViewTaskService;      -- 3 con i dati di samples/
```

Con `POLLING_APP_PASSWORD` impostata viene eseguito anche l'autotest dei permessi: 8 controlli
positivi e 4 negativi. Esito atteso:

```
=== 6/7 autotest dei permessi come utente applicativo
    ok   lettura tabella applicativa
    ok   scrittura tabella applicativa
    ok   lettura vista
    ok   invocazione funzione scalare
    ok   esecuzione stored procedure
    ok   scrittura su eProcurement
    ok   lettura anagrafica fornitori
    ok   accesso al job store Quartz
    ok   DDL negata (CREATE TABLE)
    ok   DDL negata su eProcurement
    ok   DDL negata su Quartz
    ok   scrittura su database di sistema
Autotest dei permessi: superato.
```

Conferme indipendenti sui privilegi dell'utente applicativo:

```
IS_SRVROLEMEMBER('sysadmin','PollingApp')  -> 0
IS_ROLEMEMBER('db_owner','PollingApp')     -> 0   su tutti e tre i database
ruoli server del login                     -> (nessun ruolo)
sys.sql_logins.is_policy_checked           -> 1   (CHECK_POLICY attivo)
oggetti dbo senza alcun permesso           -> (nessuno)
```
