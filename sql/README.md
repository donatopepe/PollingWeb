# sql/ — schema del database

**Questa cartella è la fonte unica di verità dello schema.** Le decisioni e le alternative
scartate sono in [`docs/adr/0001-sql-schema-source-of-truth.md`](../docs/adr/0001-sql-schema-source-of-truth.md).

```
sql/
  PollingDB/            database applicativo
    dbo/Tables/         9 tabelle
    dbo/Views/          1 vista
    dbo/Functions/      3 funzioni
    dbo/Stored Procedures/  8 procedure
    Security/           login, utente e permessi a privilegi minimi
    PollingDB.sqlproj   progetto SSDT — OPZIONALE, solo comodità su Windows
  eProcurement/         database di acquisto
    dbo/Tables/         15 tabelle
    dbo/Views/          1 vista
    dbo/Functions/      1 funzione
  Quartz/               schema del job store ADO di Quartz.NET (11 tabelle QRTZ_*)
  deploy/               ordine di applicazione e note di deploy
```

## Applicare lo schema

Il percorso supportato è lo script, non il progetto SSDT:

```bash
# prerequisito: un'istanza SQL Server raggiungibile (anche il container di questo repo)
docker compose up -d

# deploy distruttivo e idempotente su database ricreati + dati sintetici
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed
```

Dettagli, modalità `apply` e variabili d'ambiente: [`scripts/db-deploy.sh`](../scripts/db-deploy.sh)
(header del file) e [`deploy/README.md`](deploy/README.md).

## Il progetto SSDT è opzionale

`PollingDB/PollingDB.sqlproj` resta perché è comodo a chi lavora su Windows con Visual Studio.
**Non è** il percorso di deploy e **non è** un requisito di build: si compila solo con SSDT o
MSBuild di .NET Framework, quindi non è utilizzabile nella CI su Linux (il build cross-platform
fallisce con `MSB4249`).

Se il `.sqlproj` e questa cartella divergessero, **la fonte di verità sono i file `.sql`**.
Quando si aggiunge un oggetto, ricordarsi di aggiungere anche il `<Build Include>`: nessuno
strumento lo verifica.

## BOM UTF-8 nei file `.sql`

Tutti i file hanno il BOM UTF-8, eredità di Visual Studio. `sqlcmd -i` lo gestisce, ma su
**stdin** il BOM finisce nella prima istruzione e SQL Server risponde
`Msg 102: Incorrect syntax near ''`.

`scripts/db-deploy.sh` lo elimina al volo, senza modificare i file su disco, tramite la
funzione `emit_sql`. Se si aggiunge un altro percorso di deploy, tenere presente il problema:
è già costato un ciclo di debug.

## Cosa non c'è (e perché conta)

| Cosa | Stato |
|------|-------|
| Schema del database `eProcurement` | ✅ versionato in `sql/eProcurement/` (17 oggetti, sola DDL) dal task `T0.12`. Vedi il suo [`README.md`](eProcurement/README.md) |
| 4 tabelle usate da `ParserFatture.cs` (`Tab_SC00S100_*`) | **BLOCCATE**: assenti da repository, storia archiviata e snapshot di origine. Specifica delle colonne richieste e cosa chiedere al cliente in [`eProcurement/MISSING-TABLES.md`](eProcurement/MISSING-TABLES.md) (ADR 0001 §4, A6) |
| Migrazioni incrementali | assenti: gli script sono `CREATE`, non `CREATE OR ALTER`. Il deploy è per istanza nuova (`recreate` o `apply` su database vuoto) |
| Indici non-cluster | non presenti nello schema di origine, quindi non mancanti per costruzione (verificato in `T0.12`) |
