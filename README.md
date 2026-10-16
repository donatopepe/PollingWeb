# PollingWeb

Applicazione di **polling e-procurement** verso SAP: legge tracciati da una cartella
sorvegliata (richieste di acquisto, esiti ordini, anagrafiche fornitori, estrazioni di
budget, fatture), li elabora e li riversa su SQL Server, con scheduling dei processi.

> **Stato: in preparazione alla pubblicazione (`v1.0.0`).** Il repository è in fase di
> risanamento: la storia Git è già stata ripulita, ma **le credenziali non sono ancora state
> ruotate**. Leggi [`SECURITY.md`](SECURITY.md) prima di installare, clonare o pubblicare.

---

## 🔐 Sicurezza — leggi prima

| Documento | Contenuto |
|-----------|-----------|
| [`SECURITY.md`](SECURITY.md) | come segnalare una vulnerabilità, policy di divulgazione, **avviso storico e rotazione credenziali**, regole per chi contribuisce |
| [`docs/SECURITY-FINDINGS.md`](docs/SECURITY-FINDINGS.md) | inventario dei problemi trovati (gravità, posizione, stato) e **procedura operativa di rotazione** |
| [`docs/RELEASING.md`](docs/RELEASING.md) | record di audit offline, procedura di riscrittura della storia, checklist di pubblicazione |

**In sintesi:** la repository era privata e i segreti non sono stati esposti pubblicamente,
ma la rotazione è **obbligatoria prima di pubblicare** (credenziali deboli di per sé, copie
non verificabili, pubblicazione imminente). Il commit precedente resta recuperabile per SHA
sul remoto: vedi `SECURITY.md` §4.

---

## Architettura (in dieci righe)

```
SAP ──file──► cartella sorvegliata ──► motore di polling ──► SQL Server
                                          │
                                          ├── scheduling dei processi (Quartz.NET, job store ADO)
                                          ├── notifiche email di esito
                                          └── interfaccia web di amministrazione
```

| Componente | Ruolo | Tecnologia |
|------------|-------|-----------|
| `Database` | accesso dati (DataSet tipizzati, TableAdapter, filtri) | .NET Framework 4.8 → **multi-target `net48;net8.0` in corso** |
| `PollingClass` | logica di dominio: parser tracciati, flussi, job, email | idem |
| `PollingWeb` | interfaccia di amministrazione (11 pagine + 4 web service ASMX) | ASP.NET WebForms — **legacy**, in conversione a Razor Pages |
| `sql/PollingDB/` | schema del database applicativo (9 tabelle, 1 vista, 3 funzioni, 8 procedure, security) | T-SQL — **fonte unica di verità** |
| `sql/eProcurement/` | schema del database di acquisto (15 tabelle, 1 vista, 1 funzione) | T-SQL |
| `sql/Quartz/` | schema del job store ADO di Quartz.NET | T-SQL |
| `scripts/db-deploy.sh` | applica lo schema a qualunque istanza SQL Server, senza Visual Studio | Bash + `sqlcmd` |
| `sql/` | schema del job store Quartz e, a regime, fonte unica dello schema | T-SQL |

**Direzione tecnica:** il motore (worker + API) deve girare su **Linux, macOS e Windows**
senza IIS, su .NET 8. Le librerie diventano multi-target; la UI WebForms resta come *legacy
mode* opzionale e le pagine nuove si scrivono in Razor Pages. La logica di dominio non viene
riscritta.

---

## Struttura del repository

```
Database/          libreria di accesso dati
PollingClass/      logica di dominio (parser, flussi, job)
PollingWeb/        interfaccia WebForms (legacy)
sql/               schema del database: FONTE UNICA DI VERITÀ (vedi sql/README.md)
scripts/           db-deploy.sh — deploy dello schema su SQL Server
samples/           dati sintetici per test e sviluppo — mai dati reali
docker-compose.yml SQL Server 2022 per sviluppo e test
docs/              documentazione (piano, registro, sicurezza, rilascio, ADR)
PLAN.md            piano di lavorazione: 65 task, 15 criteri di accettazione
PROMPT.md          prompt operativo del loop di lavorazione
```

---

## Documentazione

| Documento | Stato |
|-----------|-------|
| [`PLAN.md`](PLAN.md) | piano completo, task, dipendenze, rischi, criteri di accettazione |
| [`docs/LOOP_LOG.md`](docs/LOOP_LOG.md) | registro delle iterazioni con evidenze |
| [`docs/SECURITY-FINDINGS.md`](docs/SECURITY-FINDINGS.md) | inventario di sicurezza |
| [`docs/RELEASING.md`](docs/RELEASING.md) | rilascio, record di audit, checklist |
| [`sql/README.md`](sql/README.md) | stato dello schema SQL e come applicarlo |
| [`sql/eProcurement/README.md`](sql/eProcurement/README.md) | schema recuperato in sola DDL, con i controlli anti-dati eseguiti |
| [`sql/eProcurement/MISSING-TABLES.md`](sql/eProcurement/MISSING-TABLES.md) | **bloccato**: 4 tabelle da richiedere all'ambiente di origine, con la specifica delle colonne |
| [`docs/adr/0001-sql-schema-source-of-truth.md`](docs/adr/0001-sql-schema-source-of-truth.md) | perché `sql/` è la fonte unica e perché i progetti SSDT non lo sono |
| [`samples/README.md`](samples/README.md) | dati sintetici e regole per aggiungerne |

Guida di installazione dettagliata, architettura, configurazione, database, operazioni, API e
troubleshooting sono **in preparazione** (fase `D` del piano). Questo `README` è un abbozzo:
il documento completo con quickstart verificato è il task `D1`.

---

## Cosa funziona già

Lo **schema del database è deployabile e verificato**:

```bash
docker compose up -d                                    # SQL Server 2022, cross-platform
POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed
#   exit 0, idempotente (verificato su due esecuzioni consecutive)

# con l'utente applicativo a privilegi minimi, che viene anche verificato:
POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] \
    ./scripts/db-deploy.sh --with-seed
#   crea il login (CHECK_POLICY ON), i permessi oggetto per oggetto e li autotesta:
#   8 controlli positivi + 4 negativi (il DDL deve risultare NEGATO su ogni database)
#   Polling     :  9 tabelle, 1 vista, 3 funzioni, 8 procedure
#   eProcurement: 15 tabelle, 1 vista, 1 funzione, 5 FK, 22 default
#   Quartz      : 11 tabelle QRTZ_*
```

Lo schema completo è versionato in [`sql/`](sql/README.md) e il deploy è idempotente:
due esecuzioni consecutive danno lo stesso risultato.

⚠️ Restano **4 tabelle** usate da `ParserFatture.cs` (`Tab_SC00S100_*`) che non esistono in
nessuna fonte disponibile: **il processo di import fatture non può funzionare** finché non
vengono recuperate dall'ambiente di origine. La specifica di ciò che serve è in
[`sql/eProcurement/MISSING-TABLES.md`](sql/eProcurement/MISSING-TABLES.md).

## Stato della build (verificato, non ipotetico)

Il progetto **non è ancora compilabile con un solo comando**. Stato accertato il 2026-10-16
con .NET SDK 8 su Linux:

```
$ dotnet build Database/Database.csproj
  error MSB3644: gli assembly di riferimento per .NETFramework,Version=v4.5 non sono stati trovati

$ dotnet build PollingClass/PollingClass.csproj
  error MSB3644: (idem)

$ dotnet build PollingWeb.sln
  PollingWeb.sln : File di soluzione error MSB4249: non è possibile compilare il progetto
  di sito Web "PollingWeb". Il compilatore ASP.NET è disponibile solo nella versione
  .NET Framework di MSBuild.                      # 2 errori, entrambi MSB4249
```

Cause e task che le risolvono:

| Sintomo | Causa | Task |
|---------|-------|------|
| `MSB3644` | i `.csproj` sono classici e dichiarano `v4.5`, senza targeting pack | `X1.1`, `X1.2`, `X1.3` (SDK-style multi-target `net48;net8.0`) |
| `MSB4249` | `PollingWeb` è un *Website project*: il compilatore ASP.NET esiste solo in MSBuild di .NET Framework, su Windows | `X1.7` (esclusione dal build CI) e `X4.2` (porting a Razor Pages) |

Oggi l'unico modo per compilare l'esistente è **Visual Studio su Windows** con il
*Developer Pack* di .NET Framework. Il build cross-platform arriva con la fase `X1`.

### Verifica già ripetibile

Le librerie compilano su `net48` (su Linux, senza Windows) una volta dichiarate come
progetto SDK-style con gli assembly di riferimento da NuGet. È stato verificato durante lo
studio iniziale e a ogni iterazione successiva:

```
Compilazione completata.  Database     -> 0 errori, 0 avvisi
Compilazione completata.  PollingClass -> 0 errori, 5 avvisi
```

Non esiste ancora uno script versionato che lo faccia: viene aggiunto in `X1.1`/`X1.2`. Il
quickstart definitivo (container SQL Server, deploy dello schema, avvio del motore su Linux e
Windows) arriva con `D1`/`D2` ed è verificato su **due** sistemi operativi.

---

## Contribuire

Prima di aprire una pull request:

1. leggi le regole per chi contribuisce in [`SECURITY.md`](SECURITY.md) §6;
2. **nessun segreto e nessun dato reale**, nemmeno negli identificatori o nei documenti;
3. il controllo obbligatorio prima del commit è descritto in `SECURITY.md` §6.

`CONTRIBUTING.md`, `CODE_OF_CONDUCT.md` e `CHANGELOG.md` sono previsti dal task `D9`.

---

## Licenza

**MIT** — vedi [`LICENSE`](LICENSE).

Le licenze dei componenti di terze parti, l'attribuzione storica e le dipendenze da rimuovere
sono in [`NOTICE`](NOTICE). L'audit completo delle licenze è il task `D10`.
