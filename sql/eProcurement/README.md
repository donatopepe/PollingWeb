# eProcurement — schema del database di acquisto

Schema versionato in **sola DDL** (nessun dato), recuperato dal task `T0.12`.

```
dbo/Tables/     15 tabelle
dbo/Views/       1 vista   (ViewEstrazioneRdARdE)
dbo/Functions/   1 funzione (StripHTML, scalare)
```

## Origine e vincolo sui dati

Lo schema esisteva solo in uno snapshot del database di produzione
(`script_eprocurement_donato.sql`), rimosso dal repository in `T0.3` perché conteneva
**15.732 istruzioni `INSERT` di dati reali**: anagrafiche fornitori, richieste di acquisto,
P.IVA, indirizzi email aziendali.

Il recupero ha estratto **esclusivamente le definizioni degli oggetti**. Per costruzione,
l'estrattore scarta i batch `INSERT` *prima* di scrivere qualunque file: nessun dato è
passato per il repository.

| Scartato | Quantità |
|---|---|
| istruzioni `INSERT` | 15.728 batch |
| blocchi `SET IDENTITY_INSERT` (righe di dati) | 4 |
| proprietà estese (metadati di diagramma di Visual Studio) | 8 |
| `USE [eProcurement]` | 1 |
| **sorgente** | 12.435 KB |
| **risultato** | **25 KB** |

Controlli eseguiti sul contenuto finale:

```
pattern del record di audit (host, login, password, marchi)  -> 0 risultati
istruzioni INSERT / SET IDENTITY_INSERT                      -> 0
indirizzi email, partite IVA                                 -> 0
valori DEFAULT presenti                                      -> (0), (1), getdate(), ('Y')
```

Le uniche occorrenze della stringa `INSERT` sono in questo file, che la descrive.

## Fedeltà rispetto all'origine

Il deploy è stato confrontato oggetto per oggetto con lo snapshot:

| Elemento | Snapshot | Dopo il deploy |
|----------|----------|----------------|
| tabelle | 15 | **15** |
| viste | 1 | **1** |
| funzioni | 1 | **1** |
| chiavi primarie | 13 | **13** |
| chiavi esterne | 5 | **5** |
| vincoli DEFAULT | 22 | **22** |
| indici non-cluster | 0 | 0 |

Nessuno scostamento.

## Perché alcune tabelle sono qui e non in `PollingDB`

`RDA_Esiti_LastOutput_OnLine` e `Tbl_Export_SAP_OnLine` erano state collocate nel database
applicativo in `T0.3`, quando erano state recuperate per non perderne la DDL. Il task `T0.12`
ha risolto l'ambiguità con due prove convergenti:

1. **Le stored procedure** le referenziano **sempre** qualificate come
   `eProcurement.dbo.*` (`Esiti_Ordini_SAP`, `Flusso_RDA*`, `Flusso_Ordini_SAP*`);
2. **I TableAdapter** che le usano ricevono `new DatabaseInfo(true)`, e il parametro `true`
   seleziona la connessione **eProcurement** (non quella applicativa).

Sono quindi state spostate qui e rimosse da `sql/PollingDB/`. Dettagli in
[ADR 0001](../../docs/adr/0001-sql-schema-source-of-truth.md) §4 (A2).

## ⚠️ Difetto latente scoperto durante lo spostamento

`sql/PollingDB/dbo/Stored Procedures/Esiti_Ordini_SAP.sql` riga 28 esegue:

```sql
INSERT INTO RDA_Esiti (Mandante, Societa, Num_Ordine, ...)   -- NON qualificata
SELECT ... FROM eProcurement.dbo.RDA_Esiti_LastOutput_OnLine -- qualificata
```

La procedura è invocata con `this.dbInfo` (connessione **applicativa**), quindi `RDA_Esiti`
non qualificata si risolve nel database `Polling`, dove la tabella **non esiste**: sta qui, in
`eProcurement`. A runtime l'istruzione fallisce con `Invalid object name 'RDA_Esiti'`.

Il difetto è **preesistente** e non è stato corretto qui: correggerlo significa scegliere se
qualificare la tabella (`eProcurement.dbo.RDA_Esiti`) o spostarla. È un problema di
correttezza SQL, tracciato nella fase `C` del piano. Non viene nascosto dal deploy, perché SQL
Server non valida gli oggetti referenziati alla creazione delle procedure: si manifesta solo
all'esecuzione.

## Cosa resta scoperto dal codice e non esiste in nessun database

Quattro tabelle sono usate da `PollingClass/ParserFatture.cs` e **non compaiono né in questo
schema né in `PollingDB`**:

```
Tab_SC00S100_1Ord      Tab_SC00S100_1Ord_R
Tab_SC00S100_2Fatt     Tab_SC00S100_2Fatt_R
```

Non erano presenti nemmeno nello snapshot: la loro DDL non è disponibile da nessuna fonte nel
record di audit. Vanno recuperate dall'ambiente di origine. Tracciate come problema aperto in
ADR 0001 §4 (A6).

## Verifica

```bash
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed

# attesi: 15 tabelle, 1 vista, 1 funzione, 5 FK, 22 default, 13 PK
sqlcmd -S <server> -d eProcurement -Q "SELECT COUNT(*) FROM sys.tables"
```

Smoke test eseguiti il 2026-10-16:

```
dbo.StripHTML(N'<p>Ciao <b>mondo</b></p>')  ->  ' Ciao  mondo'
SELECT COUNT(*) FROM ViewEstrazioneRdARdE   ->  0   (database vuoto)
```
