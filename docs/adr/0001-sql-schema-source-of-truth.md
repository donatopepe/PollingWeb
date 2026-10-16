# ADR 0001 — Fonte unica di verità per lo schema SQL

- **Stato:** accettata
- **Data:** 2026-10-16
- **Contesto del piano:** task `T0.7`
- **Sostituisce:** la situazione di fatto precedente (due progetti SSDT con contenuto duplicato)
- **File di riferimento:** [`sql/README.md`](../../sql/README.md), [`scripts/db-deploy.sh`](../../scripts/db-deploy.sh)

---

## 1. Problema

Il repository conteneva **due progetti SSDT** che dichiaravano di rappresentare lo stesso
schema, più una cartella di script:

| Percorso | Stato precedente |
|----------|------------------|
| `DatabaseSQL/` | 26 file `dbo/**` e `Security/**`; il `.sqlproj` **non conteneva alcun `<Build Include>`**: non compilava nulla, i file erano orfani |
| `PollingDB/` | 26 file **identici** a `DatabaseSQL/`; il `.sqlproj` con `<Build Include>` per tabelle, viste, procedure, funzioni e security: era l'unico che compilava |
| `sql/quartz-schema.sql` | schema del job store Quartz, in UTF-16, con nome affetto da refuso |

Conseguenze osservate, non ipotizzate:

1. **Drift reale e asimmetrico.** Il confronto hash dei 26 file ha mostrato 25 identici e **1
   diverso**: `Security/01-create-app-login.sql` era **vuoto** nel progetto che compila. La
   causa era un `sed` che leggeva e riscriveva lo stesso file (errore introdotto in `T0.2`),
   per cui il file veniva troncato prima della lettura. Effetto: il login applicativo non
   sarebbe mai stato creato da un deploy basato su `PollingDB/`, e nulla lo segnalava.
2. **Oggetti non versionati da nessuna parte.** Durante `T0.3` la cancellazione di uno
   snapshot di schema ha rischiato di far perdere la DDL di `RDA_Esiti_LastOutput_OnLine` e
   `Tbl_Export_SAP_OnLine`, usate dal codice ma assenti da entrambi i progetti.
3. **Schema mancante del tutto.** Lo schema del database `eProcurement` **non è versionato**:
   sono referenziati 19 oggetti `eProcurement.dbo.*` e solo 3 risultano definiti (e con
   collocazione ambigua). Un deploy da questo repository produce un database vuoto che le
   stored procedure non possono usare.
4. **BOM e `sqlcmd`.** Tutti i `.sql` hanno il BOM UTF-8 (eredità di Visual Studio). Passati su
   stdin a `sqlcmd` su Linux, il BOM viene interpretato come parte della prima istruzione:
   `Msg 102: Incorrect syntax near ''`. Ostacolo diretto alla distribuzione cross-platform.
5. **Vincolo di piattaforma.** I progetti SSDT si compilano solo con Visual Studio o MSBuild
   di .NET Framework su Windows. Su Linux `dotnet build PollingWeb.sln` fallisce con
   `MSB4249` (fatto `P13`). Un progetto che vuole girare su Linux non può avere il solo
   percorso di deploy dipendente da SSDT.

## 2. Decisione

1. **`sql/` è la fonte unica di verità dello schema.** La cartella è organizzata per database
   di destinazione:

   ```
   sql/
     PollingDB/       schema del database applicativo
       dbo/Tables/  dbo/Views/  dbo/Functions/  dbo/Stored Procedures/
       Security/      login, utente e permessi a privilegi minimi
       PollingDB.sqlproj
     Quartz/          schema del job store ADO
     eProcurement/    (vuota: schema non versionato — vedi §4)
     deploy/          ordine di applicazione
   ```

2. **Gli script sono autorevoli, il `.sqlproj` è opzionale.** `PollingDB.sqlproj` resta nel
   repository perché è comodo a chi lavora su Windows con Visual Studio, ma **non è** il
   percorso di deploy e non è un requisito di build. Deve limitarsi a referenziare gli stessi
   file, senza contenere definizioni proprie: se divergesse, la fonte di verità resta `sql/`.

3. **`DatabaseSQL/` è eliminato.** Dopo aver ripristinato il file vuoto, i due alberi erano
   byte-identici e il suo `.sqlproj` non compilava nulla: non conteneva informazione.

4. **Il deploy è uno script.** `scripts/db-deploy.sh` applica lo schema a qualunque istanza
   SQL Server, con due modalità:
   - `recreate` (default): elimina e ricrea i database applicativi, poi applica lo schema.
     Deterministico e idempotente, **distruttivo**: solo sviluppo e test;
   - `apply`: applica ai database esistenti senza eliminarli. Per inizializzare un'istanza
     nuova.

   Lo script funziona con `sqlcmd` locale oppure eseguendolo dentro il container di
   `docker-compose.yml`, e gestisce il BOM senza modificare i file su disco.

5. **Ordine di applicazione fissato:** tabelle → funzioni → viste → stored procedure. Le
   stored procedure non validano gli oggetti referenziati alla creazione (name resolution
   differita), quindi l'ordine conta solo per tabelle e viste.

## 3. Conseguenze

**Positive**

- Una sola copia di ogni oggetto: la divergenza silenziosa non è più possibile per
  costruzione.
- Deploy eseguibile su Linux, macOS e Windows senza Visual Studio.
- L'idempotenza è verificabile: due esecuzioni consecutive di `recreate` danno lo stesso
  risultato (verificato: exit 0 in entrambe, 23 oggetti applicativi + 11 tabelle Quartz).
- Gli oggetti non versionati sono ora un problema *esplicito* (§4), non una sorpresa al
  primo deploy.

**Negative / da gestire**

- `DatabaseSQL/` scompare: chi lo aveva nel proprio `.sln` o nei propri script deve
  aggiornare i percorsi.
- Il `.sqlproj` va mantenuto allineato a mano quando si aggiungono file: nessuno strumento lo
  verifica. Mitigazione: è opzionale, e il build di riferimento è lo script.
- `apply` non è idempotente per gli oggetti già presenti (uno script `CREATE TABLE` su tabella
  esistente fallisce). Le **migrazioni incrementali** restano un problema aperto: vedi §4.

## 4. Problemi aperti (tracciati, non risolti da questa ADR)

| # | Problema | Task |
|---|----------|------|
| A1 | Lo schema `eProcurement` non è versionato: 16 oggetti su 19 mancanti. Recuperabile come **solo DDL** dallo snapshot conservato nel record di audit offline (`script_eprocurement_donato.sql`), scartando le 15.732 istruzioni `INSERT` di dati reali | `T0.12` |
| A2 | Collocazione ambigua di `RDA_Esiti_LastOutput_OnLine` e `Tbl_Export_SAP_OnLine`: definite nel progetto del database applicativo, ma referenziate come `eProcurement.dbo.*` dalle procedure | `T0.12` |
| A3 | Nessun meccanismo di migrazione incrementale: gli script sono `CREATE` e non `CREATE OR ALTER`. Serve una strategia (script numerati, strumento di migrazione, o DACPAC) | da pianificare |
| A4 | Lo schema Quartz è quello di Quartz.NET 2.x e andrà verificato contro lo schema ufficiale della versione adottata nell'upgrade | `X2.2` |
| A5 | Lo script non crea indici non-cluster o statistiche dichiarate nei dump originali, se presenti | da verificare in `T0.12` |

## 5. Alternative considerate

| Alternativa | Perché non scelta |
|-------------|-------------------|
| Tenere `DatabaseSQL/` come fonte e completarlo | Il suo `.sqlproj` non compila nulla e non contiene `<Build Include>`: non è un progetto funzionante, e mantenerne due garantisce che il drift si ripeta |
| Un solo progetto SSDT (`PollingDB/`) senza la cartella `sql/` | SSDT è utilizzabile solo su Windows: escluderebbe la CI su Linux e contraddirebbe la direzione cross-platform |
| Eliminare i `.sqlproj` del tutto | Costo zero in termini di build (opzionale), ma perdita di comodità per chi lavora su Windows. Il vantaggio non supera il costo |
| Usare un DACPAC come artefatto di deploy | Richiede SSDT per generarlo. Rimandato: è una scelta possibile per `A3` quando il deploy cross-platform è consolidato |
| Ricostruire lo schema `eProcurement` a mano dai riferimenti nel codice | Il codice referenzia colonne in modo implicito: ricostruire i tipi sarebbe congetturale. La sorgente autorevole è lo snapshot in `T0.12` |

## 6. Verifica

Eseguita il 2026-10-16 su SQL Server 2022 in container:

```
$ POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed
  database: Polling, eProcurement, Quartz          (ricreati)
  schema applicativo: 23 file applicati            (11 tabelle, 3 funzioni, 1 vista, 8 procedure)
  job store Quartz: quartz-schema.sql              (11 tabelle QRTZ_*)
  login applicativo: saltato (POLLING_APP_PASSWORD non impostata)
  dati di esempio: samples/seed/seed.sql           (sintetici)
  exit 0

$ (seconda esecuzione identica per idempotenza)
  exit 0

# oggetti effettivamente presenti
Polling : tabelle=11 viste=1 funzioni=3 procedure=8
Quartz  : tabelle=11 (QRTZ_*)
eProcurement: tabelle=0        <- conferma il problema aperto A1

# test funzionale: la vista del seed unisce TaskService e DimProcessi
SELECT COUNT(*) FROM ViewTaskService;             -> 3
# test che distingue Datini da Datfin (le due funzioni hanno nomi fuorvianti,
# semanticamente sono "fine giornata" e "inizio giornata")
dbo.Datini('2024-03-15') -> 2024-03-15 23:59:59.997
dbo.Datfin('2024-03-15') -> 2024-03-15 00:00:00.000
```
