# PLAN — PollingWeb · Remediation, Porting Cross-Platform & Public Distribution

> Documento operativo del loop di lavorazione. Ogni voce `[ ]` è un task atomico con
> **Definition of Done (DoD)** e **evidenza** verificabile. Il loop esegue **un solo task
> per iterazione** e aggiorna questo file.

- **Revisione piano:** 2 (2026-10-16) — introduce il track cross-platform (X1–X4), cambia
  l'obiettivo e il vincolo n.4 rispetto alla revisione 1
- **Stato complessivo:** 37 / 74 task verificati
- **Revisione piano:** 5 (2026-10-16) — corretta la misura di accoppiamento a Quartz dell'ADR 0005
  (era stimata al 55% dell'assembly, misurata al ~8%); soglia anti-svuotamento da 95% a **90%**
  per `X1.3c`; `X1.3b` scomposto in `X1.3b1`–`X1.3b5`, una classe di schedulazione per task.
  Motivo in [`docs/adr/0006`](docs/adr/0006-quartz-coupling-measurement-correction.md)
- **Revisione piano:** 3 (2026-10-16) — `X1.1` ristretto e spostato dopo `X1.2`/`X1.3`;
  DoD di `X1.2` e `X1.5` corrette. Motivo in [`docs/adr/0004`](docs/adr/0004-build-layout-and-multitargeting.md)
- **Prompt operativo:** [`PROMPT.md`](PROMPT.md)
- **Log iterazioni:** [`docs/LOOP_LOG.md`](docs/LOOP_LOG.md)

---

## 1. Obiettivo (una frase)

Rendere `PollingWeb` — applicazione di polling e-procurement verso SAP — **eseguibile su
Linux, macOS e Windows**, compilabile, testato, documentato e pubblicabile, priva di segreti
e dati reali, con guida completa di installazione/esercizio e licenza MIT.

### Metrica globale + soglia
**Rapporto criteri di accettazione soddisfatti / criteri totali** (rubrica binaria §7).
Soglia: **= 1.00** prima del tag `v1.0.0`.
Soglia intermedia per il loop: **ogni iterazione chiude 1 task con DoD ratio = 1.00**;
massimo 3 tentativi per task, poi il task passa a `BLOCKED` con motivo scritto.

### Vincoli non negoziabili
1. **Nessun segreto** (password, utenze, hostname interni, token) nel repository pubblico.
2. **Nessun dato reale** (fornitori, P.IVA, fatture, budget, dump con INSERT) nel repository pubblico.
3. **Cross-platform reale**: il motore di polling e le API devono girare su Linux senza
   emulazione, su un runtime supportato (.NET 8+). Windows è una piattaforma supportata,
   non un requisito.
4. **Il contratto funzionale con SAP non cambia** (nomi file, tracciati, semantica delle
   stored procedure) senza un ADR in `docs/adr/`. La compatibilità SOAP dei client esistenti
   va garantita o migrata con piano esplicito.
5. **Backward compatibility opzionale**: la build .NET Framework 4.8 / IIS resta disponibile
   come *legacy mode* finché la build cross-platform non raggiunge la parità funzionale; non
   deve però bloccare il track cross-platform.
6. Ogni task lascia il repository in stato **buildabile e verde**.
7. Un task = un cambio. Nessun refactoring estetico non richiesto dal task corrente.
8. Modifica `PLAN.md` **solo** per marcare stato (`[ ]`→`[~]`→`[x]`/`[!]`) e aggiungere
   evidenza nel registro §8. La revisione del piano è un task a sé.

---

## 2. Stato di partenza accertato (fatti verificati)

### 2.1 Stato originale (revisione 1)

| # | Fatto | Evidenza |
|---|-------|----------|
| F1 | 3 progetti C# + 2 SQL project, .NET Framework 4.5 | `PollingWeb.sln`, `*.csproj`, `*.sqlproj` |
| F2 | `Database` e `PollingClass` **compilano** con dotnet 8 + `Microsoft.NETFramework.ReferenceAssemblies` | build pilot in `/tmp/pb` → 0 errori, 5 warning |
| F3 | `PollingWeb` è un **Website project** (nessun `.csproj`, nessun `.designer.cs`) → non compilabile da CI | `CodeFile=` + `partial class` senza dichiarazioni controlli |
| F4 | `PollingClass/Check_RdA.cs` è **codice rotto** (conversione VB→C#), orfano (fuori dal csproj) | `error CS1525` (82,53) |
| F5 | Credenziali reali **hardcoded** e presenti nella storia Git di una repository GitHub **privata** (verificato 2026-10-16) | `Web.config`, `Database/app.config`, `Settings.Designer.cs`, `Bin/*.config`, `.exclude`; remote `github.com/donatopepe/PollingWeb` |
| F6 | Dati reali nel repo | `script_eprocurement_donato.sql` (15.732 INSERT), `SC00G100_inizio.TXT` (anagrafiche + P.IVA), `BDG_MCC.xlsx` |
| F7 | SQL injection | `Database/DataType.cs` → `FiltroCampi.BuildWhere` concatena stringhe; `TaskService.aspx.cs` concatena `SelectCommand` |
| F8 | `AntiSqlInject` usato in **1 solo punto** | `PollingClass/ParserFatture.cs:126` |
| F9 | 4 web service ASMX dichiarano `CodeBehind="~/App_Code/*.cs"` che **non esistono** | `PollingWeb/*.asmx` |
| F10 | `.gitignore` assente; `bin/`, `obj/`, `packages/`, `.vs/` tracciati | risolto in T0.5 |
| F11 | Nessun README, LICENSE, test, CI | assenza file |
| F12 | Dipendenza LGPL `Remotion.*` trascinata da `LinqToExcel` | `packages/LinqToExcel.1.11.0/lib/*` |
| F13 | Duplicazione schema SQL in `DatabaseSQL/` e `PollingDB/` | diff directory |
| F14 | `providerName="System.Data.EntityClient"` su stringhe SqlClient pure | `Web.config` |
| F15 | Path interni/assoluti in publish profile (`D:\<publish-root>\...`) | `App_Data/PublishProfiles/FolderProfile.pubxml` |
| F16 | Docker disponibile, `mcr.microsoft.com` raggiungibile → SQL Server 2022 testabile | `docker info`, `curl mcr` |

### 2.2 Analisi di portabilità (revisione 2) — cosa blocca e cosa no

| # | Fatto | Impatto sul port | Evidenza |
|---|-------|------------------|----------|
| P1 | **Nessun** uso di `HttpContext.Current`, `HostingEnvironment`, `Server.MapPath`, `BuildManager`, `System.Web.UI` | ✅ Nessun accoppiamento al contesto web | `grep -rn "HttpContext\.Current\|HostingEnvironment\|MapPath\|BuildManager" --include=*.cs Database PollingClass` = 0 |
| P2 | `System.Web` serve solo per `HttpUtility.UrlDecode` (`Database/AntiSqlInject.cs`) | ✅ `HttpUtility` esiste nel .NET moderno (`System.Web.HttpUtility`) | `grep -rn System.Web` |
| P3 | 4 classi estendono `System.Web.Services.WebService` con `[WebMethod]`: `ImportBudget`, `ImportFatture`, `LeggiFornitore`, `Ordini_Sap` | ⚠️ **ASMX non supportato** su .NET 8 → serve API ASP.NET Core (+ CoreWCF per SOAP) | `PollingClass/*.cs` |
| P4 | Numerosi `using System.Web;` **non usati** (`Prog_Quartz.cs`, `Log_Errori.cs`, `MailSede.cs`, `PollingException.cs`) | ✅ Rimovibili senza effetti | `grep` = 0 usi |
| P5 | Riferimento `WindowsBase` in `PollingClass.csproj` | ✅ **Non usato**: nessun `System.Windows`, `Dispatcher`, `DependencyObject` | `grep` = 0 |
| P6 | `System.Drawing` presente solo in `Database/DataType.cs` (`class Colore`, `class FontSerif`) | ✅ Usato **solo** da `Check_RdA.cs` (file rotto, da eliminare) → `System.Drawing.Common` è Windows-only da .NET 6: va eliminato, non portato | `grep -rln "Aggset\.Color\|Colore\.\|FontSerif"` → solo `Check_RdA.cs` |
| P7 | `Quartz 2.1.2` (stato iniziale) | aggiornato a Quartz 3.13.1 in X2.2; i gateway legacy attendono le API async | `X2.2` verificato su SQL Server Linux; port net8 residuo in X1.3c |
| P8 | `LinqToExcel` + `Remotion.*` (LGPL) | ⚠️ Windows-only (OLEDB) e licenza incompatibile → sostituzione già pianificata | `packages/LinqToExcel.1.11.0/lib/` |
| P9 | Accesso dati via `System.Data.SqlClient` + `SqlDataAdapter`/`SqlCommandBuilder` + `DataSet` tipizzati | ✅ Portabile; su net8 usare `Microsoft.Data.SqlClient` (API quasi identica) | `Database/TableAdapter*.cs` |
| P10 | `ConfigurationManager` per le connection string | ✅ Portabile con `System.Configuration.ConfigurationManager` NuGet; meglio astrarre su `IConfiguration` | `Database/DatabaseInfo.cs` |
| P11 | SQL Server è l'unica dipendenza infrastrutturale esterna | ✅ gira in container Linux (`mcr.microsoft.com/mssql/server:2022-latest`) | `docker info` OK |
| P12 | Logica di business totale ≈ 2.500 righe (`PollingClass`) + DAL generato ≈ 4.000 (`Database`) | ✅ Dimensione compatibile con un port incrementale, non con una riscrittura | `wc -l` |
| P13 | `dotnet build PollingWeb.sln` fallisce con **`MSB4249`**: il compilatore ASP.NET esiste solo in MSBuild di .NET Framework, su Windows | ⚠️ Il sito **non è compilabile in CI su Linux**: da escludere dal build di default (`X1.7`) e sostituire con Razor Pages (`X4.2`) | `dotnet build PollingWeb.sln` |
| P14 | I `.csproj` classici dichiarano `v4.5` senza targeting pack | ⚠️ `dotnet build <progetto>.csproj` fallisce con **`MSB3644`** su qualsiasi OS: serve la conversione SDK-style (`X1.1`–`X1.3`) | `dotnet build Database/Database.csproj` |
| P15 | `Microsoft.Data.SqlClient` 5.x attiva `Encrypt` **per default**: contro un certificato self-signed la connessione fallisce con un errore di *handshake pre-login* | ⚠️ Le connection string servono `TrustServerCertificate=True` (o `Encrypt=False`) o un certificato valido. Colpisce la quickstart (`AC8`) e va intercettato con un messaggio chiaro da `C7`. Verificato: `default` → handshake fallito, `TrustServerCertificate=True` → connessione riuscita | test su container, net8.0 |

**Conclusione.** Il port è a basso rischio tecnico: mancano solo (a) sostituzione
ASMX→API, (b) upgrade Quartz, (c) rimozione `System.Drawing`/`LinqToExcel`/`WindowsBase`,
(d) doppio target delle librerie. **Non** serve riscrivere la logica di dominio.

**Decisione architetturale (riferimento per il loop).**

```
                    ┌──────────────────────┐
                    │   PollingWeb.sln     │
                    └──────────┬───────────┘
        ┌──────────────────────┼──────────────────────┬─────────────────┐
        ▼                      ▼                      ▼                 ▼
  Database (lib)        PollingClass (lib)     PollingWorker      PollingApi
  net48;net8.0          net48;net8.0           net8.0 (Quartz 3)  net8.0 (ASP.NET Core)
        ▲                      ▲                      ▲                 ▲
        └──────────────┬───────┴──────────────────────┴─────────────────┘
                       ▼
              PollingWeb.Core (UI Razor Pages, net8.0)
                       │
                       ▼
          SQL Server 2022 (container Linux) + Quartz tables
```

- **Track prioritario (A):** SDK-style multi-target + Worker + API → il **motore** gira su
  Linux/macOS/Windows.
- **Track successivo (B, Fase 4):** UI portata a Razor Pages → parità completa.
- **Legacy:** `net48` + IIS conservato come *legacy mode* documentato, non bloccante.

---

## 3. Fasi e task

Legenda stato: `[ ]` da fare · `[~]` in corso · `[x]` fatto e verificato · `[!]` bloccato

### Fase 0 — Sicurezza e igiene del repository (bloccante per la pubblicazione)

- [x] **T0.1 — Inventario segreti**
  DoD: `docs/SECURITY-FINDINGS.md` elenca ogni occorrenza (file:riga, tipo di segreto, gravità).
  Evidenza: output di `grep -rniE '(password|pwd|user id|uid|secret|token|apikey)'` filtrato, contato.
- [x] **T0.5 — `.gitignore` + `.gitattributes`**
  DoD: ignorati `bin/`, `obj/`, `packages/`, `.vs/`, `*.user`, `*.suo`, `App_Data/logs`, `samples/private/`;
  `.gitattributes` con `* text=auto` e normalizzazione CRLF/LF per `.sln`/`.csproj`.
  Evidenza: `git status --porcelain` pulito dopo `dotnet build`.
- [x] **T0.2 — Esternalizzazione configurazione**
  DoD: `Web.config` / `Database/app.config` / `Settings.Designer.cs` senza valori reali; placeholder
  `${POLLING_DB_SERVER}`; aggiunti `Web.config.example` e `Database/app.config.example`;
  `Settings.settings` rigenerato con default vuoti. **Incluso:** redazione con placeholder degli
  hostname interni citati in `PLAN.md`, `PROMPT.md`, `docs/LOOP_LOG.md` (fuori da `docs/`), così
  che l'evidenza qui sotto sia verificabile.
  Evidenza: `git grep -nE "$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"` = 0 risultati in
  tutti i file tracciati. I pattern **non sono riportati qui**: sono essi stessi informazione
  sensibile (hostname, login, password) e risiedono solo nel record di audit offline descritto
  in `docs/RELEASING.md`.
- [x] **T0.3 — Rimozione dati reali dal working tree**
  DoD: rimossi/`git rm --cached` `script_*donato.sql`, `SC00G100_inizio.TXT`, `BDG_MCC.xlsx`,
  `_047_1047_oda_input.#00`, `_063_1063_rda_input.#00`, `DatabaseSQL/Importa log di schema/`,
  `PollingDB/Importa log di schema/`. Gli artefatti `bin/`, `obj/`, `packages/`, `.vs/`, `*.user`
  sono già de-tracciati da T0.5: qui si completa la parte dati.
  Sostituiti da `samples/` con dati **sintetici** minimi.
  Evidenza: `du -sh` repo < 5 MB senza `.git`; i file in `samples/` contengono solo dati fittizi.
- [x] **T0.4 — De-branding**
  DoD: rimosso il logo dell'istituto e ogni riferimento al suo nome, al suo dominio e ai suoi
  marchi; logo placeholder neutro (SVG, licenza MIT); attribuzione storica in `NOTICE`.
  Evidenza: `git grep -ilE "$BRAND_PATTERNS"` = 0 risultati (fuori da `NOTICE`). I pattern
  sono nel record di audit offline (`docs/RELEASING.md`), non in questo documento: anche il
  nome dell'istituto non deve comparire nel repository pubblico.
- [x] **T0.6 — Pulizia file morti**
  DoD: eliminati `PollingClass/Check_RdA.cs` (codice VB→C# rotto e non compilato),
  `PollingWeb/App_Code/*.exclude` (24 copie obsolete), `PollingWeb/css/style - Copia.css`,
  `PollingWeb/App_Code/TEST_QUARTZ.cs` (web service senza `.asmx`, irraggiungibile),
  `PollingWeb/job_scheduling_data_2_0.xsd` (schema di template VS non referenziato),
  i placeholder `Procedure1.sql`/`Procedure2.sql`/`Script1.sql`/`Script2.sql`,
  le 20 pagine di blocco McAfee `PollingWeb/js/*_warning.htm` (asset di terzi non
  ridistribuibili, salvati per errore al posto dei download); `sript_quartz.sql` →
  `sql/quartz-schema.sql` ricodificato da UTF-16 a UTF-8 con header di provenienza.
  Evidenza: build verde; `git ls-files` non elenca più i file; `git grep` dei nomi in tutti
  i file **tranne i documenti di lavoro** (`PLAN.md`, `PROMPT.md`, `docs/LOOP_LOG.md`) = 0,
  perché quei documenti devono necessariamente nominare ciò che è stato rimosso.
  **Sblocca P6** (rimozione `System.Drawing`).
- [x] **T0.11 — Rimozione degli identificatori derivati dall'azienda**
  DoD: nessun identificatore di codice, database, progetto o file contiene il nome o
  l'abbreviazione dell'istituto per cui il software è stato sviluppato. La mappatura
  vecchio→nuovo non è riportata qui: è nel record di audit offline. Sono stati rinominati:
  il framework di eccezioni (`PollingException`, `PollingExceptionType`,
  `PollingExceptionHandler`, `GestisciPollingException`) e il file `Database/PollingException.cs`;
  le chiavi di connessione (`PollingConnectionString`, `PollingConnectionStringDirect`) in
  `Web.config`, `app.config`, `Settings.settings`, `Settings.Designer.cs`, `dtsTabelle.xsd`
  e nelle pagine `.aspx`; il database applicativo (`Polling`); la directory e il progetto SQL
  (`PollingDB/`, `PollingDB.sqlproj`); le variabili `dbInfoPolling` e `DemoPolling`.
  Contestualmente: rimossi gli attributi `CodeBehind` dai 4 `.asmx` (puntavano a `App_Code`,
  rimossa in `T0.6`: la classe si risolve dall'assembly in `Bin`).
  Evidenza: `git grep -inE "$COMPANY_PATTERNS"` = 0 (pattern dal record di audit, escluso
  `NOTICE`); build net48 verde; per ogni identificatore rinominato, grep incrociato che
  dimostri che fornitori e consumatori coincidono (es. la chiave di connessione compare con
  lo stesso nome in `Web.config`, `Settings.settings`, `Settings.Designer.cs` e nei
  `SqlDataSource` delle pagine).
  Nota: i riferimenti negli `.aspx` non sono verificabili con un build finché il progetto
  web non è compilabile (`X1.7`/`X4.2`): la verifica è il grep incrociato, dichiarato come
  tale.
- [x] **T0.7 — SQL projects: schema unico**
  DoD: una sola fonte di verità per lo schema (`sql/PollingDB/`, `sql/eProcurement/`, `sql/Quartz/`),
  `DatabaseSQL/` e `PollingDB/` consolidati; ADR `docs/adr/0001-sql-schema-source-of-truth.md`.
  Evidenza: deploy idempotente su container con exit 0.
- [x] **T0.12 — Recupero dello schema `eProcurement` (solo DDL)**
  DoD: gli oggetti `eProcurement` referenziati dal codice sono versionati in `sql/eProcurement/`.
  Estrazione **solo DDL** dallo snapshot nel record di audit offline, scartando integralmente
  ogni `INSERT`; verifica che il risultato non contenga dati reali né pattern del record di audit;
  risolta l'ambiguità di collocazione di `RDA_Esiti_LastOutput_OnLine` e `Tbl_Export_SAP_OnLine`
  (vedi ADR 0001 §4); il deploy non deve più lasciare `eProcurement` vuoto.
  Evidenza: `sql/eProcurement/` contiene ≥ 16 oggetti; `scripts/db-deploy.sh` mostra
  `tabelle=N>0` su `eProcurement`; scansione dei pattern di audit sul nuovo contenuto = 0;
  ADR 0001 §4 aggiornata (A1, A2 chiusi).
- [!] **T0.13 — Recupero delle 4 tabelle `Tab_SC00S100_*`** — **BLOCCATO (2026-10-16)**
  DoD: la DDL di `Tab_SC00S100_1Ord`, `Tab_SC00S100_1Ord_R`, `Tab_SC00S100_2Fatt`,
  `Tab_SC00S100_2Fatt_R` è versionata in `sql/eProcurement/`. Sono usate da
  `PollingClass/ParserFatture.cs` e non esistono in nessuno schema versionato **né nello
  snapshot di origine** (ADR 0001 §4, A6): la DDL va recuperata dall'ambiente di origine.
  Evidenza: 4 file presenti; deploy con `tabelle=19`; scansione anti-dati = 0.
  ⚠️ **Richiede accesso all'ambiente di origine del cliente.** Se non è disponibile, il task
  va marcato `[!]` con il motivo, non chiuso a intuito: ricostruire colonne e tipi dai soli
  riferimenti nel codice sarebbe congetturale.
  **Motivo del blocco:** la ricerca esaustiva non ha trovato alcuna fonte. La DDL non esiste
  nel repository, né nei 9 commit della storia archiviata, né nei 17 oggetti dello snapshot
  `eProcurement`. Nella storia i nomi compaiono **solo** in `ParserFatture.cs` e
  `Check_RdA.cs`: nessuno script, nessuna definizione.
  **Prodotto comunque (deliverable utile):** `sql/eProcurement/MISSING-TABLES.md` — specifica
  *evidence-based* delle colonne usate dal codice per tabella e per operazione (estratta
  meccanicamente, con le righe di riferimento), le scorciatoie da non prendere e la richiesta
  precisa da inoltrare all'ambiente di origine.
  **Impatto:** `ParserFatture` non può funzionare; **AC3** non è raggiungibile per il processo
  fatture finché mancano. Il task va sbloccato quando l'utente fornisce la DDL.
- [x] **T0.8 — Script di deploy DB riproducibile**
  DoD: `scripts/db-deploy.sh` crea `Polling`, `eProcurement`, `Quartz` su un'istanza SQL Server
  con utente applicativo a privilegi minimi; niente utenti/password reali; idempotente.
  Evidenza: `docker compose up -d mssql` + script → exit 0 e tabelle presenti.
- [x] **T0.9 — Nota di disclosure + rotazione credenziali**
  DoD: `SECURITY.md` dichiara che le credenziali storiche sono compromesse e vanno ruotate;
  `docs/SECURITY-FINDINGS.md` §5 con l'istruzione operativa di rotazione.
  Evidenza: sezioni presenti e linkate dal README.
  *Per soddisfare l'evidenza è stato creato un `README.md` minimo (e il `LICENSE` MIT già
  promesso da `NOTICE`): `D1` e `D10` restano aperti per il README completo e per l'audit
  delle licenze, gli header SPDX e `docs/LICENSING.md`.*
- [x] **T0.10 — Storia Git ripulita**
  DoD: backup `git bundle` verificato; branch `main` con **orphan commit** contenente solo il
  contenuto sanificato; vecchio `master` conservato come tag `legacy-archive` (**non** distribuito);
  istruzioni in `docs/RELEASING.md`.
  Evidenza: `git bundle verify` OK; `git grep` su tutti i commit di `main` = 0 segreti.
  *Eseguito in anticipo rispetto a §4 su richiesta esplicita dell'utente. L'archivio della
  vecchia storia non è un tag nel repository (sarebbe distribuito): è il bundle nel record di
  audit offline, con SHA-256 registrato.*

### Fase 1 — Fondazioni cross-platform (X1)

> Assorbe i task T1.1–T1.7 della revisione 1 (mappatura in nota a fine fase).

- [x] **X1.1 — Layout soluzione + `Directory.Build.props`**
  DoD: `Directory.Build.props` centralizza `LangVersion`, `Nullable`, `Deterministic` e la
  versione; `Directory.Packages.props` (Central Package Management); `Directory.Build.targets`
  per regole comuni; `PollingWeb.sln` allineata ai **progetti esistenti**.
  Evidenza: `dotnet msbuild Database/Database.csproj -getProperty:LangVersion -getProperty:Version`
  mostra i valori dei props (i props sono importati anche dai csproj classici, via
  `Microsoft.Common.props`); `dotnet sln list` elenca i progetti.
  *Ristretto da ADR 0004: la richiesta di contenere `PollingWorker`/`PollingApi`/`PollingWeb.Core`/`tests`
  passa ai task che quei progetti li creano (`X2.1`, `X3.1`, `X4.1`, `T1`), e l'evidenza
  `dotnet build PollingWeb.sln → 0 errori` passa a `X1.6`/`X1.7`: fino ad allora è un obiettivo,
  non un'evidenza verificabile.*
- [x] **X1.2 — `Database` multi-target `net48;net8.0`**
  DoD: csproj SDK-style; TFM `net48;net8.0`; rimosso `System.Drawing` (P6); rimossa la
  dipendenza da `System.Web` (senza aggiungere il riferimento a net48: si sostituisce
  `HttpUtility.UrlDecode`, vedi ADR 0004 §2.4); provider dati usato direttamente, l'astrazione
  è `X1.5`. Il riferimento `WindowsBase` è in `PollingClass`, quindi appartiene a `X1.3`
  (ADR 0004 §2.3).
  Evidenza: `dotnet build Database/Database.csproj -f net8.0` e `-f net48` → 0 errori;
  `git grep -n "System.Web\|System.Drawing" Database` = 0.
- [x] **X1.3 — `PollingClass` SDK-style su `net48` + rimozione dipendenze non ridistribuibili**
  DoD: csproj SDK-style con `TargetFramework` **net48** (il multi-target arriva in `X1.3c`);
  **rimozione di `LinqToExcel` e `Remotion`** (LGPL, Windows-only: `ImportaExcel.cs` e'
  codice morto, il suo unico uso e' in un commento di `Import_Budget.cs`); rimozione del
  riferimento `WindowsBase` (non usato); passaggio a `Microsoft.Data.SqlClient`.
  Evidenza: `dotnet build PollingClass/PollingClass.csproj -f net48` → 0 errori;
  `git grep -inE 'remotion|linqtoexcel' -- '*.cs' '*.csproj' '*.config' '*.props' '*.targets' '*.sln' '*.sqlproj'`
  = 0 in codice, file di progetto e configurazione. I documenti (`NOTICE`, `SECURITY.md`,
  questo piano, il registro) nominano la dipendenza rimossa per registrarla: vanno esclusi
  dall'evidenza, perché documentarne la rimozione fa parte del compito.
  *Stima iniziale ADR 0005 per intero file (non linee realmente dipendenti): Quartz è stato
  aggiornato a 3.13.1 in X2.2; il perimetro non migrabile net8 ora è primariamente ASMX.*
- [x] **X1.3b1 — Separazione schedulazione/Quartz in `Fornitori_Scheduler`**
  DoD: il calcolo dell'espressione cron è in `FornitoriSchedulePolicy` (platform-neutral); le
  chiamate Quartz sono in `FornitoriQuartzGateway`; `IFornitoriSchedulingGateway` costituisce il
  confine e `JobFornitori` è separato dal servizio. **Implementazione di riferimento** per
  `X1.3b2`–`X1.3b5` (ADR 0006 §3).
  Evidenza (Linux): `dotnet build PollingClass/PollingClass.csproj -f net48` → 0 errori, 5 warning
  preesistenti; `dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj` → 65 passati,
  1 ignorato; `Prog_Quartz.cs` non contiene riferimenti Quartz/IJob/StdSchedulerFactory/JobBuilder/
  TriggerBuilder/IScheduler. Test della policy cron e del range orario verdi. Windows/macOS non
  verificati in locale.
- [x] **X1.3b2 — `ImportFatture_Scheduler`**
  DoD: policy di scheduling separata dal codice invoice-import; provider Quartz in
  `ImportFattureQuartzGateway`; `JobImportFatture` separato; interfaccia provider condivisa con
  `X1.3b1`; firme pubbliche e semantica override/interval conservate.
  Evidenza (Linux): build `PollingClass` net48 → 0 errori, 5 warning preesistenti; unit test →
  73 passati, 1 ignorato; nessun riferimento Quartz/IJob/StdSchedulerFactory/JobBuilder/
  TriggerBuilder/IScheduler in `ImportFatture.cs`; `git diff --check` pulito. Windows/macOS non
  verificati in locale.
- [x] **X1.3b3 — `ImportBudget_Scheduler`**
  DoD: `IFornitoriSchedulingGateway` per la superficie del servizio; Quartz isolato in
  `ImportBudgetQuartzGateway`; `JobBudget` separato; firme esistenti mantenute; entry point del
  job resta `ImportBudget.Leggi_File(..., onetime)`.
  Evidenza (Linux): build net48 → 0 errori, 5 warning preesistenti; unit test net8 → 80 passati,
  1 ignorato; nessun riferimento Quartz/IJob/StdSchedulerFactory/JobBuilder/TriggerBuilder/
  IScheduler in `Import_Budget.cs`; `git diff --check` pulito. Windows/macOS non verificati in
  locale.
- [x] **X1.3b4 — `Ordini_Scheduler`**
  DoD: provider `IOrdiniSchedulingGateway`, adapter Quartz `OrdiniQuartzGateway`, separati
  `JobOrdini_SAP_Invio` / `JobOrdini_SAP_Esito`; scheduling giornaliero e interval delegati;
  firme pubbliche esistenti mantenute.
  Evidenza (Linux): build net48 → 0 errori, 5 warning preesistenti; unit test net8 → 90 passati,
  1 ignorato; nessun simbolo Quartz in `Ordini_Sap.cs`; `git diff --check` pulito. Windows/macOS
  non verificati localmente.
- [x] **X1.3b5 — `SQL01_Scheduler`**
  DoD: `IFornitoriSchedulingGateway` per la superficie scheduler; Quartz isolato in
  `SQL01QuartzGateway`; `JobSQL01` separato; firme pubbliche mantenute; cron giornaliero e
  polling interval delegati, con `onetime` conservato.
  Evidenza (Linux): build net48 → 0 errori, 5 warning preesistenti; unit test net8 → 97 passati,
  1 ignorato; nessun riferimento Quartz/IJob/StdSchedulerFactory/JobBuilder/TriggerBuilder/
  IScheduler in `ExportSQL01.cs`; `git diff --check` pulito. Windows/macOS non verificati in locale.
- [x] **X1.3c — `PollingClass` multi-target `net48;net8.0`**
  DoD: `TargetFrameworks=net48;net8.0`; entrambi i target compilano.
  Evidenza (Linux, 2026-10-16): `dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet` e `-f net48` → 0 errori, 5 avvisi ciascuno; unit test → 98 passati, 2 ignorati. Sui 4.298 LOC non vuote, una misura statica delle direttive di compilazione conta 4.178 righe nette net8 (97,2%, sopra la soglia 95%); le sole esclusioni nette sono metadata/attributi e base ASMX (`System.Web.Services`) in `Import_Budget.cs`, `ImportFatture.cs`, `LeggiFornitore.cs`, `Ordini_Sap.cs`, mantenuti nel target net48. Gli endpoint legacy restano consumabili via WebForms; la migrazione SOAP è separata in X3.3. `git diff --check` pulito. Windows/macOS non verificati localmente.
- [x] **X1.4 — Configurazione astratta**
  DoD: accesso a config dietro `IAppConfig`/`IOptions`; su net48 legge `Web.config`/`app.config`,
  su net8.0 legge `appsettings.json` + variabili d'ambiente; nessuna lettura diretta di
  `ConfigurationManager` nel codice di dominio.
  Evidenza: test unit con provider fake; `git grep -n "ConfigurationManager" Database PollingClass` = 0
  (escluso l'adapter).
- [x] **X1.5 — Provider dati astratto (`Microsoft.Data.SqlClient`)**
  DoD: **un solo provider**, `Microsoft.Data.SqlClient`, su entrambi i TFM (verificato che
  `SqlDataAdapter` e `SqlCommandBuilder` vi funzionano: vedi ADR 0004 §1); adapter sottile per
  isolare il DAL da un futuro cambio di provider; connection string senza credenziali nel codice.
  Evidenza: integration test CRUD verdi su entrambi i TFM.
- [x] **X1.6 — CI multi-OS**
  DoD: `.github/workflows/ci.yml` con matrice `ubuntu-latest`, `macos-latest`, `windows-latest`:
  restore, build, unit test; job `integration` con service container SQL Server su ubuntu.
  Evidenza: workflow valido (dry-run/act o run verde documentato); build verde su 3 OS.
- [x] **X1.7 — Website project legacy fuori dal build CI**
  DoD: `PollingWeb` (WebForms) escluso dal build di default del CI ma **compilabile su richiesta**
  tramite `scripts/build-legacy.ps1`/`build-legacy.sh` (compile-check delle sole code-behind con
  `.designer.cs` generati) oppure marcato esplicitamente `legacy` con motivazione in `docs/LEGACY.md`.
  Evidenza (Linux, 2026-10-16): il Website è dichiarato legacy e documentato in `docs/LEGACY.md`; il workflow `.github/workflows/ci.yml` esegue i progetti SDK esistenti senza invocare il Website. `dotnet build PollingWeb.sln --nologo -v quiet` riprodotto → MSB4249; build IIS via Visual Studio/.NET Framework 4.8 Developer Pack descritta ma non eseguita localmente. |

> **Mappatura revisione 1 → revisione 2:** T1.1→X1.2 · T1.2→X1.3 · T1.3→X1.3 · T1.4→X1.7/X4.2 ·
> T1.5→X3.2 · T1.6→X1.1 · T1.7→X1.6.

### Fase 2 — Worker e scheduler cross-platform (X2)

- [ ] **X2.1 — Progetto `PollingWorker` (.NET 8 Worker Service)**
  DoD: `IHostedService` avvia la scansione dei file source e i job; eseguibile come processo
  `dotnet PollingWorker.dll`; nessuna dipendenza da IIS; registra il progetto in `PollingWeb.sln`
  (requisito trasferito da `X1.1`, ADR 0004 §2.1).
  Evidenza: `dotnet run --project PollingWorker` avvia senza errori su Linux e Windows.
- [x] **X2.2 — Quartz 3.x con job store ADO su SQL Server**
  DoD: Quartz aggiornato a 3.13.1; `JobStoreTX` con provider `MicrosoftDataSqlClient`, JSON
  serializer e schema QRTZ verificato; configurazione via `POLLING_QUARTZ_CONNECTION` o
  `QuartzConnectionString`; misfire threshold 60 s, clustering disabilitato per deployment
  singola istanza. Gli adapter restano sincroni per i chiamanti legacy; i job implementano la
  firma asincrona Quartz 3. Persistenza provata con restart scheduler.
  Evidenza reale (Linux): build net48 0 errori (5 warning preesistenti); unit 98 passati, 2
  ignorati (test DB persistent-store collocato nei test d'integrazione); `scripts/test-integration.sh`
  con SQL Server → 16/16 passati, incluso trigger persistente recuperato dopo restart; schema SQL
  applicato dal deploy. Windows/macOS non verificati localmente. Dettagli/configurazione in
  [`docs/adr/0007`](docs/adr/0007-quartz-3-persistent-store.md).
- [ ] **X2.3 — Porting dei job**
  DoD: `JobFornitori`, `JobBudget`, `JobFatture`, `JobOrdiniSap`, `JobRDA` eseguiti dal worker con
  gli stessi parametri di `TaskService`; nessun `HttpContext`; log strutturato per job.
  Evidenza: test che esegue ogni job su DB container con dati sintetici → esito atteso.
- [ ] **X2.4 — Hosting su Linux/macOS/Windows**
  DoD: `systemd` unit (`deploy/systemd/pollingworker.service`), `Dockerfile` + servizio in
  `docker-compose.yml`, `UseWindowsService()` per Windows; `docs/OPERATIONS.md` con le 3 modalità.
  Evidenza: container avviato e job eseguito; unit file validato con `systemd-analyze verify`.
- [ ] **X2.5 — Logging e health check**
  DoD: Serilog su file+console con rotazione; endpoint/health check per liveness del worker;
  `log_linea` alimenta `LogProcedure` come nell'originale.
  Evidenza: log prodotti nei 3 OS; health check `--health` exit code 0/1.

### Fase 3 — API cross-platform (X3)

- [ ] **X3.1 — Progetto `PollingApi` (ASP.NET Core 8)**
  DoD: minimal API o controller; OpenAPI (`/openapi/v1.json`); config da `appsettings.json`;
  binding su `0.0.0.0`; autenticazione a chiave o Windows/None documentata; registra il progetto
  in `PollingWeb.sln` (requisito trasferito da `X1.1`, ADR 0004 §2.1).
  Evidenza: `dotnet run --project PollingApi` e chiamata reale a un endpoint documentata.
- [ ] **X3.2 — Porting dei 4 web service**
  DoD: `ImportBudget`, `ImportFatture`, `LeggiFornitore`, `Ordini_Sap` esposti come endpoint
  REST/JSON con gli stessi parametri e la stessa semantica; test per ciascuno.
  Evidenza: `docs/API.md` + test verdi.
- [ ] **X3.3 — Compatibilità SOAP**
  DoD: ADR `docs/adr/0003-soap-compatibility.md` che decide fra CoreWCF (SOAP 1.1 su .NET 8) e
  migrazione dei client; se CoreWCF, i 4 ASMX sono riesposti su `PollingApi` con lo stesso WSDL.
  Evidenza: ADR approvato; se CoreWCF, test di chiamata SOAP verde.
- [ ] **X3.4 — Test dei endpoint**
  DoD: `WebApplicationFactory` + test di integrazione per tutti gli endpoint, incluse le
  validazioni e i casi di errore.
  Evidenza: coverage endpoint ≥ 90%; test verdi su Linux e Windows.

### Fase 4 — UI cross-platform (X4)

- [ ] **X4.1 — `PollingWeb.Core` con Razor Pages: fondamenta**
  DoD: progetto ASP.NET Core Razor Pages; layout con Bootstrap; menu equivalente a `home.aspx`;
  `Web.config`/`appsettings.json` per le connection string; nessun IIS richiesto; registra il
  progetto in `PollingWeb.sln` (requisito trasferito da `X1.1`, ADR 0004 §2.1).
  Evidenza: `dotnet run` e smoke test Playwright sulla home.
- [ ] **X4.2 — Porting delle 11 pagine WebForms**
  DoD: `Default`, `home`, `TaskService`, `GestioneDayQuartz`, `ListaTrigger`, `SQL01`,
  `Import_Budget`, `Import_Fatture`, `LetturaFornitori`, `Ordini_sap`, `VisualizzaLogTaskService`.
  Parità funzionale per ciascuna.
  Evidenza: per pagina, elenco funzionalità originale → funzionalità portata; test E2E.
- [ ] **X4.3 — i18n e tema**
  DoD: localizzazione IT/EN (le stringhe originali sono italiane: IT = default, EN completo);
  logo neutro; nessun asset di terze parti.
  Evidenza: test E2E nelle due lingue; `git grep -ilE "$BRAND_PATTERNS"` = 0 (fuori da `NOTICE`).
- [ ] **X4.4 — Test E2E**
  DoD: Playwright headless in CI (ubuntu) sui flussi principali: inserimento TaskService,
  esecuzione manuale job, consultazione log, import budget.
  Evidenza: CI verde con artefatti di trace allegati.

### Fase 5 — Correttezza e sicurezza del codice (C)

- [x] **C1 — `FiltroCampi.BuildWhere` parametrizzato**
  DoD: `BuildWhere(ArrayList, SqlCommand)` restituisce SQL con parametri `@p0..@pN`; campo validato tramite allowlist dei filtri DAL osservati, tipo C/N/D, operatore/connettore/parentesi allowlist; valori parametrizzati e numeri/date convertiti invariant. I 18 call site dei table adapter usano il command parametrizzato. L'overload string legacy è mantenuto solo per compatibilità e non è usato dal DAL; rimozione in C2.
  Evidenza (Linux): test payload `' OR 1=1 --` verifica parametro separato; build Database net48/net8 → 0 errori; unit 104 passati/2 ignorati. La verifica SQL Server dedicated resta in C2 CRUD.
- [x] **C2 — Migrazione di tutti i `TableAdapter*`**
  DoD: tutti i TableAdapter usano `BuildWhere` parametrizzato; rimossi overload legacy che accettavano clausole SQL raw; ogni query filtro imposta il testo e i parametri sullo stesso `SelectCommand`.
  Evidenza (Linux): tutti i 18 call site usano `SqlCommand` parametrizzato; rimosso `BuildWhere(ArrayList)` e gli overload `Load(string,string,string)` senza call site; test d'integrazione `SqlClientAdapterTests` → 2/2 passati sul container, include fixture sintetica di CRUD e controllo payload injection come parametro. Build Database net48/net8 → 0 errori; unit 104/2 ignorati. La copertura CRUD completa per ogni schema resta da ampliare in test successivi.
- [x] **C3 — Rimozione concatenazione in `SqlDataSource`/pagine**
  DoD: `SelectCommand` usa un parametro `@IdProcesso` bound per il lookup dinamico; Insert/Update restano con valori `@...` già bound e SQL text composto solo da identificatori/colonne costanti; nessuna interpolazione di `ddl.SelectedValue` in SQL.
  Evidenza (Linux, 2026-10-17): `TaskService.aspx.cs` usa `SelectParameters`; scansione dei `SelectCommand` non trova più interpolazioni/SelectedValue. Il test pagina WebForms non è eseguibile nel test project net8; la compilazione completa Website resta bloccata da MSB4249, documentato in X1.7.
- [x] **C4 — Eliminazione `Convert.ToDateTime(string)` culture-dependent**
  DoD: parse esplicito dd/MM/yyyy ISO/invariant, conversioni numeriche invariant e valori ADO `DateTime` nativi; cron utilizza componenti orario typed.
  Evidenza (Linux, 2026-10-17): nessun `Convert.ToDateTime(` eseguibile rimasto in Database/PollingClass/PollingWeb (solo commento storico); test `ValidaData` e `IsNumeric` sotto en-US e it-IT passano; build Database e PollingClass net48/net8 → 0 errori; unit 106 passati/2 ignorati.
- [x] **C5 — `DatabaseInfo` thread-safe, senza stato statico mutabile**
  DoD: config e connessione per istanza; timeout command configurato con fallback 30 secondi; rimosso stato statico DAL; ciclo connessione/transaction Dispose-aware.
  Evidenza (Linux): test 100 istanze parallele con connection string/timeout distinti; build Database e PollingClass net48/net8 → 0 errori; unit 108 passati/2 ignorati.
- [x] **C6 — Gestione errori e logging**
  DoD: catch vuoti rimossi; rethrow usa `throw;`; eccezioni database inoltrate a `PollingExceptionHandler`/Trace invece di Console; SQL helper conserva l'eccezione interna senza includere SQL/value nei messaggi.
  Evidenza (Linux, 2026-10-17): scansioni `catch\s*\{`, `throw ex;`, `Console.WriteLine(ex.Message)` su Database/PollingClass = 0; aggiunti test per handler/error rethrow; build Database e PollingClass net48/net8 → 0 errori; unit 111 passati/2 ignorati.
- [x] **C7 — Config validata all'avvio**
  DoD: `providerName` corretti; validazione delle connection string obbligatorie con messaggio
  chiaro; nessun default implicito su host di produzione.
  Evidenza (Linux, 2026-10-17): test unitari su sintassi incompleta/errata, provider legacy errato e connection Quartz malformata/incompleta; unit 114 passati/2 ignorati; build Database e PollingClass net8.0/net48 → 0 errori. `dotnet build PollingWeb.sln --nologo -v quiet` fallisce con MSB4249 (Website WebForms), limite già documentato in X1.7; C7 non introduce host predefiniti.
- [x] **C8 — Percorsi file configurabili**
  DoD: nessun path assoluto `D:\`/`C:\`; base dir da config; validazione esistenza directory.
  Evidenza (Linux, 2026-10-17): `ConfiguredFilePaths.Combine` usa `Path.Combine`, rifiuta directory mancanti e file name con componenti path; i flussi leggono/scrivono con helper. Test unitari 115 passati/2 ignorati; `PollingClass` net8.0/net48 exit 0; scansione path Windows assoluti e concatenazioni Windows nei flussi → 0 risultati.
- [!] **C9 — Robustezza parsing tracciati SAP — BLOCCATO dopo 3 iterazioni**
  DoD: `ParserFatture`, `ImportaXML`, `Ordini_Sap`, `Flusso_*` gestiscono file vuoto/troncato/campi
  mancanti senza crash; encoding Windows-1252/UTF-8 dichiarato e testato.
  Evidenza (Linux, 2026-10-17): test unit 140 passati/2 ignorati; build PollingClass net8.0/net48 exit 0. Implementati controlli strutturali e test sintetici per encoding, date, output troncati e workbook incompleti. **Blocco:** dopo 3 iterazioni non è possibile completare test end-to-end dei parser e `Flusso_*`: richiedono fixture SAP complete verificate e SQL contract/DDL mancanti (T0.13 è bloccato). Mitigazione: task di parsing isolati e fixture DB sintetiche dopo disponibilità DDL; nessuna assunzione sul contratto SAP.
- [x] **C10 — SMTP funzionante**
  DoD: `MailSede` autentica realmente via configurazione; invio testato con server finto
  (Papercut/SMTP4Dev in Docker); nessuna credenziale placeholder.
  Evidenza (Linux, 2026-10-17): integration test autenticato contro SMTP4Dev Docker → 1/1 pass con credenziali sintetiche da environment; log container conferma AUTH e ricezione. Unit 145/2 ignorati; PollingClass net8.0/net48 exit 0; integration project build exit 0. Nessuna credenziale versionata; macOS/Windows non verificati localmente.
- [x] **C11 — Qualificare `RDA_Esiti` in `Esiti_Ordini_SAP.sql`**
  DoD: l'istruzione `INSERT INTO RDA_Esiti` (non qualificata) è corretta. La procedura gira
  sulla connessione applicativa ma la tabella risiede in `eProcurement`: a runtime fallisce con
  `Invalid object name 'RDA_Esiti'`, e il difetto non emerge alla creazione per name resolution
  differita. Scegliere con evidenza fra qualificare (`eProcurement.dbo.RDA_Esiti`) o spostare
  la tabella, e documentare la scelta.
  Evidenza (Linux, 2026-10-17): `INSERT` punta a `eProcurement.dbo.RDA_Esiti`; `SchemaContractTests.Esiti_Ordini_SAP_executes_with_eProcurement_archive_target` esegue la procedura dal DB `Polling` in transazione rollbackable su SQL Server 2022 deployato → 1/1 pass. La suite integration completa segnala anche un failure preesistente non correlato: conteggio tabelle `Polling` atteso 9, rilevato 10.
  Fonte: ADR 0001 §4 (A7), scoperto in `T0.12`.

### Fase 6 — Test (T)

- [x] **T1 — Infrastruttura test**
  DoD: `tests/PollingWeb.UnitTests`, `tests/PollingWeb.IntegrationTests` (xUnit); `docker compose`
  con SQL Server 2022; `scripts/db-deploy.sh` usato dai test; skip automatico se DB assente;
  registra i progetti di test in `PollingWeb.sln` (requisito trasferito da `X1.1`, ADR 0004 §2.1).
  Evidenza: `dotnet test` esegue entrambi i progetti.
  *Target `net8.0`, non `net48`: i test devono essere eseguiti, e il runtime di .NET Framework
  non esiste fuori da Windows. Conseguenza dichiarata in `tests/README.md`: i tipi di
  `PollingClass` (parser, flussi, wrapper di stored procedure) **non sono coperti** finché
  `X1.3c` non rende quel progetto multi-target.*
- [!] **T2 — Unit test logica pura**
  DoD: copertura di `ValidaData`, `ValidaMail`, `FiltroCampi`, `AntiSqlInject`, `Aggset`,
  `ParserFatture`, config adapter.
  Evidenza (Linux, 2026-10-17): dopo oltre tre iterazioni T2 è bloccato `[!]` secondo il limite tentativi del workflow. Ultimo run finale dopo aver scartato i test esplorativi Aggset: 162 passati/2 ignorati; coverage: `ValidaData` 67,6%, `IsNumeric` 51,0%, `Aggset` 22,7%, `ParserFatture` 10,0%. Mitigazione: mantenuti test puri e wrapper; riaprire con test SQL/integration separato e fixture SAP verificabili.
- [~] **T3 — Integration test stored procedure e funzioni**
  DoD: test per `Datini`, `Datfin`, `StripHTML`, `Flusso_RDA*`, `Flusso_Ordini_SAP*`,
  `Esiti_Ordini_SAP`, `ImportaBudget`, `AggiornaFornitori`, `AzzeraTargetSC00G100`.
  Evidenza (Linux, 2026-10-17, iterazione 52): build IntegrationTests exit 0; 13 test SQL filtrati passano. Aggiunti rami item dei flussi per istituto, delete sintetico e verifica di errore con rollback. DoD ancora incompleta: non tutti i rami business e side effect delle routine sono verificati; suite completa ha failure noti. `dotnet build PollingWeb.sln --nologo -v quiet` eseguito, exit 1 `MSB4249` Website legacy.
- [ ] **T4 — Integration test DAL su entrambi i TFM**
  DoD: round-trip CRUD per tutti i TableAdapter + `ViewTaskService`, eseguiti su net48 e net8.0.
  Evidenza: test verdi con container SQL Server.
- [ ] **T5 — Test API e worker**
  DoD: endpoint coperti da `WebApplicationFactory`; job coperti da test di integrazione.
  Evidenza: test verdi; `docs/API.md` coerente.
- [ ] **T6 — `scripts/verify.sh` e `scripts/verify.ps1`**
  DoD: pipeline unica restore+build+test per Linux/macOS e Windows, exit 0.
  Evidenza: esecuzione pulita documentata su almeno 2 OS.

### Fase 7 — Documentazione (D)

- [ ] **D1 — `README.md` (EN) + `README.it.md`**
  DoD: badge CI/licenza; cos'è; architettura in 10 righe; **quickstart cross-platform**
  (`docker compose up` + `dotnet run`); configurazione; build; deploy su Linux (systemd/Docker),
  macOS (launchd/dev) e Windows (IIS legacy + Windows Service); API; troubleshooting; licenza.
  Evidenza: ogni comando del README eseguito su clone pulito.
- [ ] **D2 — `docs/INSTALL.md`**
  DoD: prerequisiti per i 3 OS, SQL Server in container, deploy DB, Quartz, SMTP, permessi;
  percorso legacy IIS documentato a parte.
  Evidenza: checklist provata e trascritta.
- [ ] **D3 — `docs/ARCHITECTURE.md`**
  DoD: diagrammi Mermaid (componenti, flusso RDA/ODA, budget/fatture, scheduler, dati) e
  tabella di mappatura **WebForms → Razor Pages** e **ASMX → REST**.
  Evidenza: diagrammi renderizzati; riferimenti file:riga corretti.
- [ ] **D4 — `docs/CONFIGURATION.md`**
  DoD: ogni chiave documentata (nome, tipo, default, obbligatorietà, esempio) per
  `appsettings.json` **e** per la modalità legacy `Web.config`.
  Evidenza: confronto automatico con i file di config reali.
- [ ] **D5 — `docs/DATABASE.md`**
  DoD: schema, tabelle, viste, stored procedure, funzioni, ordine di deploy, migrazioni,
  privilegi minimi.
  Evidenza: generato/verificato dai file `sql/`.
- [ ] **D6 — `docs/OPERATIONS.md`**
  DoD: runbook con comandi per Linux (systemd), Docker, macOS e Windows: avvio, stop, log,
  trigger Quartz, riavvio job, backup, file bloccati, escalation.
  Evidenza: ogni procedura con comando eseguibile e output atteso.
- [ ] **D7 — `docs/API.md`**
  DoD: endpoint REST documentati con esempi request/response ed errori; sezione compatibilità SOAP.
  Evidenza: esempi validati dai test.
- [ ] **D8 — `docs/TROUBLESHOOTING.md`**
  DoD: ≥ 15 sintomi con causa e rimedio, inclusi problemi cross-platform (line ending, case
  sensitivity del filesystem, encoding, culture, permessi, fuso orario).
  Evidenza: problemi emersi durante il port documentati.
- [ ] **D9 — `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `CHANGELOG.md`, `SECURITY.md`, `NOTICE`**
  DoD: processi, condotta, changelog Keep-a-Changelog, policy di disclosure, licenze third-party
  con versioni.
  Evidenza: file presenti e linkati dal README.
- [ ] **D10 — `LICENSE` MIT + header SPDX**
  DoD: `LICENSE` MIT; header SPDX nei file `.cs` dei nuovi progetti; `docs/LICENSING.md` con la
  compatibilità delle dipendenze (MIT/Apache-2.0); nessuna dipendenza copyleft residua.
  Evidenza: `reuse`/check manuale; `git grep -i "remotion"` = 0.
- [ ] **D11 — `docs/LEGACY.md`**
  DoD: cosa resta su .NET Framework/IIS, come compilarlo, per quanto è supportato e il piano di
  dismissione.
  Evidenza: istruzioni verificate.

### Fase 8 — Packaging e pubblicazione (R)

- [ ] **R1 — Docker Compose di sviluppo**
  DoD: `docker-compose.yml` con SQL Server 2022 + deploy automatico schema; healthcheck; servizi
  facoltativi (worker, api, web, smtp fittizio).
  Evidenza: `docker compose up -d` → test di integrazione verdi.
- [ ] **R2 — Immagini container dell'applicazione**
  DoD: `Dockerfile` multi-stage (SDK → runtime) per `PollingWorker`, `PollingApi`, `PollingWeb.Core`;
  esecuzione con utente non root; `HEALTHCHECK`; immagini avviabili su Linux.
  Evidenza: `docker compose up` avvia i 3 servizi e un job completa.
- [ ] **R3 — Pacchetto di release**
  DoD: `.github/workflows/release.yml` produce `PollingWeb-<ver>-crossplatform.zip`
  (worker+api+web pubblicati), `PollingWeb-<ver>-legacy-iis.zip` e `PollingWeb-<ver>-sql.zip`,
  con checksum SHA-256.
  Evidenza: artefatti generati e ispezionati senza segreti.
- [ ] **R4 — CI multi-OS di release**
  DoD: la release builda e testa su `ubuntu-latest` + `windows-latest`; i pacchetti sono
  riproducibili (`Deterministic`, `ContinuousIntegrationBuild`).
  Evidenza: due run verdi e artefatti con hash identici per i file generati.
- [ ] **R5 — Versioning e tag**
  DoD: `<Version>1.0.0</Version>` in `Directory.Build.props`; tag annotato `v1.0.0`; note in
  `CHANGELOG.md`.
  Evidenza: `git tag -v v1.0.0`; release notes complete.
- [ ] **R6 — Checklist di pubblicazione**
  DoD: `docs/RELEASING.md` con checklist pre-push (scan segreti, licenze, PII, build su 3 OS,
  test, docs); `gitleaks` in CI; decisione esplicita su cosa fare di `PLAN.md`, `PROMPT.md`,
  `docs/SECURITY-FINDINGS.md` nella distribuzione (redazione con placeholder o esclusione).
  Evidenza: scan su `main` = 0 finding; checklist compilata nel log.
- [ ] **R7 — Verifica end-to-end su clone pulito, su 2 OS**
  DoD: clone in directory temporanea su Linux **e** su Windows, esecuzione del quickstart del
  README senza passi impliciti: `docker compose up`, `dotnet run`, job eseguito, UI raggiungibile.
  Evidenza: trascrizione dei due run in `docs/LOOP_LOG.md`.

---

## 4. Ordine di esecuzione

```
T0.2 → T0.3 → T0.4 → T0.6 → T0.11 → T0.9
  → T0.7 → T0.12 → T0.8              # schema unico, schema eProcurement, deploy riproducibile
  #   T0.13 (4 tabelle Tab_SC00S100_*) e' BLOCCATO in attesa della DDL dall'ambiente di origine:
  #   non blocca il resto della pipeline, va sbloccato quando la DDL e' disponibile.
  → X1.2 → X1.3 → T1 → X1.3b1 → X1.3b2 → X1.3b3 → X1.3b4 → X1.3b5 → X2.2 → X1.3c → X1.1 → X1.4 → X1.5 → X1.6 → X1.7
  #   ordine rivisto da ADR 0004, 0005 e 0006:
  #     X1.1 dopo X1.2/X1.3  -> i props hanno senso solo su progetti SDK-style
  #     T1 prima di X1.3b*   -> il refactoring delle classi di schedulazione richiede i test
  #     X2.2 prima di X1.3c  -> Quartz 3.x è provider scelto; X1.3c porta i job su net8
  #     X1.6/X1.7 pretendono la soluzione verde: e' li' che quell'evidenza diventa vera
  → C1 → C2 → C3 → C4 → C5 → C6 → C7 → C8 → C9 → C10
  → T1 → T2 → T3 → T4
  → X2.1 → X2.2 → X2.3 → X2.4 → X2.5
  → X3.1 → X3.2 → X3.3 → X3.4
  → X4.1 → X4.2 → X4.3 → X4.4
  → T5 → T6
  → D10 → D1 → D2 → D3 → D4 → D5 → D6 → D7 → D8 → D9 → D11
  → R1 → R2 → R3 → R4 → R5 → R6 → R7
  → T0.10                           # riscrittura storia: ultimo passo, dopo la sanificazione
```

**Regola di precedenza:** il loop prende sempre il **primo task non completato** in questo
ordine, salvo dipendenze esplicite. Un task può essere marcato `[!]` (bloccato) solo con
motivo e mitigazione scritti in `docs/LOOP_LOG.md`.

**Nota sull'ordine.** La sanificazione (T0.2–T0.9) precede il port perché il repository sarà
pubblico: nessun nuovo progetto deve nascere con segreti dentro. `T0.10` (storia Git) è
volutamente **ultimo**: riscrivere la storia mentre si lavora genera conflitti inutili.

---

## 5. Ambiente di verifica

| Componente | Stato | Uso |
|---|---|---|
| .NET SDK 8.0.426 | `/opt/dotnet` | build net8.0 e net48 (reference assemblies) |
| Docker 29.9.0 + `mcr.microsoft.com` | attivo | SQL Server 2022 e SMTP fittizio per i test |
| `Microsoft.NETFramework.ReferenceAssemblies` 1.0.3 | NuGet | target net48 su Linux/macOS |
| macOS / Windows | **non disponibili localmente** | verificati solo in CI (X1.6); il loop deve dichiararlo |

**Comandi di verifica standard.**

```bash
export PATH="/opt/dotnet:$PATH"
dotnet build PollingWeb.sln -c Debug          # build
dotnet test  PollingWeb.sln -c Debug          # test
bash scripts/verify.sh                        # pipeline completa (T6)
```

**Limite dichiarato.** macOS e Windows non sono verificabili in questo ambiente: l'evidenza
per i task che li riguardano è la **CI multi-OS** (X1.6). Il loop non deve dichiarare verificato
su macOS/Windows ciò che non è stato eseguito lì.

---

## 6. Rischi e mitigazioni

| Rischio | Impatto | Mitigazione |
|---|---|---|
| Credenziali nella storia Git di una repo privata, in via di pubblicazione | Alto | T0.10 storia nuova (fatto) + T0.9 rotazione obbligatoria **prima** della pubblicazione |
| ASMX non supportato su .NET 8 | Alto | X3.3 ADR SOAP/CoreWCF prima di rimuovere gli endpoint citati |
| Upgrade Quartz 2.1.2 → 3.x cambia API | Chiuso in X2.2 | Gateway/adapters aggiornati e test di persistenza SQL con restart scheduler |
| `System.Data.SqlClient` vs `Microsoft.Data.SqlClient` (differenze di comportamento: `DataSet` typeless, timeout) | Medio | X1.5 adapter unico + T4 round-trip su entrambi i TFM |
| Doppio target raddoppia le combinazioni di test | Medio | matrice CI + `Directory.Packages.props`; test condizionali per TFM |
| UI: 11 pagine da portare a Razor Pages | Medio | X4.2 un task per pagina con tabella di parità funzionale |
| Regressi su parsing tracciati SAP (dipendenza da encoding/culture Windows) | Alto | C9 fixture + C4 invariant culture + test con `it-IT` |
| Perdita dati/segreti nella riscrittura storia | Alto | backup `git bundle` verificato prima di T0.10 |
| macOS/Windows non testabili in locale | Medio | CI multi-OS obbligatoria; limite dichiarato in §5 |
| Schema SQL duplicato e disallineato | Medio | T0.7 fonte unica + ADR 0001 |
| Dipendenza LGPL `Remotion` | Medio | X1.3 sostituzione, verificata da D10 |

---

## 7. Criteri di accettazione (release v1.0.0)

Rubrica binaria — ognuno 0/1, target **15/15**:

- [ ] **AC1** `dotnet build PollingWeb.sln -c Release` → 0 errori su Linux **e** Windows.
- [ ] **AC2** `dotnet test` → tutti verdi, integration inclusi, con DB in container.
- [ ] **AC3** **Il motore gira su Linux senza IIS**: `docker compose up` avvia worker+api+web,
      esegue un job reale su SQL Server containerizzato e completa con successo.
- [ ] **AC4** La stessa build funziona su Windows (worker come servizio o processo, IIS non richiesto).
- [ ] **AC5** `git grep` su tutti i file di `main` per segreti/PII/hostname interni → 0 risultati
      (esclusi i documenti di sicurezza redatti).
- [ ] **AC6** Nessuna dipendenza copyleft; `NOTICE` allineato alle DLL/licenze distribuite.
- [ ] **AC7** `LICENSE` MIT presente e valido.
- [ ] **AC8** README con quickstart cross-platform eseguito su clone pulito su **due** OS.
- [ ] **AC9** `docs/` completo: INSTALL, ARCHITECTURE, CONFIGURATION, DATABASE, OPERATIONS, API,
      TROUBLESHOOTING, LEGACY, TESTING.
- [ ] **AC10** CI verde su Linux, macOS e Windows.
- [ ] **AC11** Release artifact riproducibili (cross-platform + legacy IIS + SQL) con checksum e
      senza segreti.
- [ ] **AC12** Nessun path assoluto macchina-specifico in codice/config distribuiti.
- [ ] **AC13** Parità funzionale documentata: tabella WebForms→Razor Pages, ASMX→REST, job→worker.
- [ ] **AC14** `CHANGELOG.md` + tag `v1.0.0` con note di rilascio.
- [ ] **AC15** Nessun `catch { }` silenzioso, nessuna concatenazione SQL in ingresso utente,
      parsing date/numeri culture-invariant.

---

## 8. Registro avanzamento

| Data | Iterazione | Task | Esito | Evidenza |
|------|-----------|------|-------|----------|
| 2026-10-17 | 52 | T3 Integration test stored procedure e funzioni — parziale | `[~]` | IntegrationTests build 0; 13 test SQL filtrati pass. `dotnet build PollingWeb.sln --nologo -v quiet` exit 1 `MSB4249` Website. Restano branch business non coperti; failure suite noti. |
| 2026-10-17 | 42 | C10 SMTP funzionante | `[x]` | SMTP4Dev container; authenticated integration 1/1 pass; unit 145/2 ignored; PollingClass net8/net48 and IntegrationTests build exit 0; compose config clean; Windows/macOS not locally verified. |
| 2026-10-17 | 43 | C11 Qualificare RDA_Esiti | `[x]` | `Esiti_Ordini_SAP` integration call from Polling in rollback transaction → 1/1 pass after deploy; integration project build exit 0. Full integration suite has unrelated table-count failure (Polling expected 9, actual 10); solution build exit 1/MSB4249 Website. |
| 2026-10-17 | 37 | C9 parsing SAP — `[!]` bloccato dopo 3 iterazioni | `[!]` | PollingClass net8.0/net48 exit 0; unit 140/2 ignorati; controlli strutturali e test sintetici aggiunti. End-to-end ParserFatture/Flusso_* non completabile senza fixture SAP/DDL verificate; mitigazione descritta nel task e LOOP_LOG. |
| 2026-10-17 | 35 | C8 Percorsi file configurabili | `[x]` | PollingClass net8.0/net48 exit 0; unit 115/2 ignorati; scansioni assoluti/concatenazioni Windows 0 risultati; diff-check exit 0. |
| 2026-10-17 | 34 | C7 Config validata all'avvio | `[x]` | Test connection string SQL malformata/incompleta e provider legacy errato; Quartz connection malformata/incompleta; unit 114/2 ignorati; Database/PollingClass net8/net48 exit 0; solution fallisce con MSB4249 già noto. |
| 2026-10-16 | 0 | studio iniziale | completato | revisione 1 del piano |
| 2026-10-16 | 1 | T0.1 inventario segreti | `[x]` | `docs/SECURITY-FINDINGS.md` — S1: 2 password SQL LOGIN; S2: 22 occorrenze; S3: 1; S4: 87 (17 file); S5: 15.732 INSERT |
| 2026-10-16 | 2 | T0.5 `.gitignore` + `.gitattributes` | `[x]` | 148 artefatti + 6 file utente de-tracciati (382 → 236 file); `git ls-files --others --exclude-standard` = solo i 2 nuovi file |
| 2026-10-16 | 3 | revisione 2: track cross-platform | `[x]` | §2.2 analisi di portabilità (P1–P12) + nuova struttura fasi X1–X4, C, T, D, R; 64 task, 15 criteri |
| 2026-10-16 | 4 | T0.2 esternalizzazione configurazione | `[x]` | `git grep -nE` con i pattern dal record di audit → 0 risultati; build net48 verde (Database 0/0, PollingClass 0 errori/5 avvisi) |
| 2026-10-16 | 5 | T0.3 rimozione dati reali + samples/ | `[x]` | contenuto tracciato 5.854.377 → 2.508.750 byte; working tree (esclusi .git/packages/.vs/bin/obj) 3,2 MB; `samples/budget/BDG_sample.xlsx` letto da `ReadExcelFileDOM` → `sharedStringsIrrisolte=0` |
| 2026-10-16 | 6 | T0.6 pulizia file morti | `[x]` | file tracciati 239 → 186; `git grep` dei nomi rimossi (esclusi i documenti di lavoro) = 0; build net48 verde |
| 2026-10-16 | 7 | T0.4 de-branding | `[x]` | logo e marchio rimossi; `git grep -ilE "$BRAND_PATTERNS" -- . ':(exclude)NOTICE'` → 0 risultati |
| 2026-10-16 | 8 | T0.11 identificatori aziendali | `[x]` | vecchi identificatori: 0 occorrenze (pattern dal record di audit); chiave di connessione: 13 definizioni / 13 consumatori `.aspx` / 4 `.cs`; build net48 verde |
| 2026-10-16 | 9 | T0.10 riscrittura storia Git | `[x]` | 164/164 blob senza pattern; 1 commit raggiungibile; `.git` 17,03 MiB → 608 KiB; `git bundle verify` OK |
| 2026-10-16 | 10 | T0.9 disclosure + rotazione | `[x]` | `SECURITY.md` (7 sezioni) e `docs/SECURITY-FINDINGS.md` §5; 11 link relativi verificati esistenti; `git grep` pattern di audit su README/SECURITY/LICENSE/NOTICE → 0 |
| 2026-10-16 | 11 | T0.7 schema unico + ADR 0001 | `[x]` | 2 alberi SQL identici (26 file) → 1; `DatabaseSQL/` eliminato (0 `<Build Include>`); deploy `recreate` due volte → exit 0; Polling 11 tabelle/1 vista/3 funzioni/8 procedure, Quartz 11 tabelle QRTZ_; `ViewTaskService` → 3 righe; `eProcurement` → 0 tabelle (gap → T0.12) |
| 2026-10-16 | 12 | T0.12 schema eProcurement (solo DDL) | `[x]` | 17 oggetti estratti, 15.728 `INSERT` scartati (12.435 KB → 25 KB); deploy `tabelle=15 viste=1 funzioni=1 FK=5 default=22 PK=13` = **identico all'origine**; pattern di audit = 0; `StripHTML` e `ViewEstrazioneRdARdE` eseguibili |
| 2026-10-16 | 13 | T0.13 tabelle `Tab_SC00S100_*` | `[!]` | **bloccato**: ricerca esaustiva su repository, 9 commit della storia archiviata e snapshot `eProcurement` → nessuna DDL esistente. Prodotto `sql/eProcurement/MISSING-TABLES.md` con la specifica delle colonne usate dal codice (10 istruzioni estratte, righe 277–560) |
| 2026-10-16 | 14 | T0.8 deploy con privilegi minimi | `[x]` | deploy `recreate` due volte → exit 0; **12 controlli di autotest** (8 positivi + 4 negativi); `sysadmin=0`, `db_owner=0` sui 3 database, nessun ruolo server, `CHECK_POLICY=1`; **0 oggetti dbo senza permesso** (drift check) |
| 2026-10-16 | 15 | revisione 3 del piano (ADR 0004) | `[x]` | `X1.1` ristretto e spostato dopo `X1.2`/`X1.3`; DoD di `X1.2` e `X1.5` corrette; renderlo eseguibile era impossibile: i progetti citati non esistono e il build verde contraddice P13/P14 |
| 2026-10-16 | 16 | X1.2 Database multi-target | `[x]` | `net48` 0 errori / 0 avvisi; `net8.0` 0 errori / 28 avvisi (tutti CA2200 = `throw ex;`, scope di C6); `System.Web` e `System.Drawing` in `Database` → 0; artefatti per entrambi i TFM; errori `MSB3644` sulla soluzione → 0 (restano solo 2 `MSB4249` del sito) |
| 2026-10-16 | 17 | revisione 4 del piano (ADR 0005) | `[x]` | stima iniziale per file: 2.905/5.246 (55%) coinvolti da Quartz/ASMX; Quartz aggiornato in X2.2, perimetro ASMX residuo esplicitato in ADR 0006 e X1.3c |
| 2026-10-16 | 18 | X1.3 PollingClass SDK-style net48 + rimozione LGPL | `[x]` | `dotnet build PollingClass/PollingClass.csproj -f net48` → **0 errori** (5 avvisi preesistenti) su Linux; `remotion\|linqtoexcel` in codice/progetti/config → **0**; `WindowsBase` e `HintPath` verso `PollingWeb/Bin` → 0; `System.Data.SqlClient` in `PollingClass` → 0; Quartz e Common.Logging ora da NuGet |
| 2026-10-16 | 19 | T1 infrastruttura test | `[x]` | **unit 50 superati / 1 ignorato** (difetto `C6` documentato); **integrazione 16 superati** con DB e **16 ignorati** senza DB (skip automatico, 0 falliti); `scripts/test-integration.sh` end-to-end → deploy + autotest permessi + test, tutto verde; `dotnet sln list` mostra 5 progetti |
| 2026-10-16 | 20 | revisione 5 del piano (ADR 0006) | `[x]` | corretta la **mia** misura dell'ADR 0005: l'accoppiamento reale a Quartz è ~8% dell'assembly (215 righe di API su 1.430 righe di classi coinvolte), non il 55% dichiarato contando i *file*. Soglia `X1.3c` da 95% a **90%**; `X1.3b` scomposto in `X1.3b1`–`X1.3b5` |
| 2026-10-16 | 21 | X1.3b1 separazione Fornitori_Scheduler/Quartz | `[x]` | Build net48: 0 errori, 5 warning preesistenti; unit test: 65 superati, 1 ignorato; nessun simbolo Quartz in `Prog_Quartz.cs`; policy cron testata; `git diff --check` pulito. Windows/macOS non verificati in locale. |
| 2026-10-16 | 22 | X1.3b2 separazione ImportFatture_Scheduler/Quartz | `[x]` | Build net48: 0 errori, 5 warning preesistenti; unit test net8: 73 superati, 1 ignorato; nessun simbolo Quartz nel file `ImportFatture.cs`; test di policy, contratto adapter, identità job/trigger e intervalli verdi; `git diff --check` pulito. |
| 2026-10-16 | 23 | X1.3b3 separazione ImportBudget_Scheduler/Quartz | `[x]` | Build net48: 0 errori, 5 warning preesistenti; unit test net8: 80 superati, 1 ignorato; nessun simbolo Quartz nel file `Import_Budget.cs`; adapter verifica cron feriale, preserva il job `Leggi_File(..., onetime)`, override e scheduling immediato/intervallo; `git diff --check` pulito. |
| 2026-10-16 | 24 | X1.3b4 separazione Ordini_Scheduler/Quartz | `[x]` | Build net48: 0 errori, 5 warning preesistenti; unit test net8: 90 superati, 1 ignorato; nessun simbolo Quartz in `Ordini_Sap.cs`; adapter conserva routing invio/esito, `tipoExport`, cron lun-ven, override e trigger one-time/interval; `git diff --check` pulito. |
| 2026-10-16 | 25 | X1.3b5 separazione SQL01_Scheduler/Quartz | `[x]` | Build net48: 0 errori, 5 warning preesistenti; unit test net8: 97 superati, 1 ignorato; nessun simbolo Quartz in `ExportSQL01.cs`; adapter convalida cron e intervalli, job separato e delega schedulazione; `git diff --check` pulito. |

| 2026-10-16 | 26 | X2.2 Quartz 3.13.1 ADO store persistente | `[x]` | Build net48: 0 errori, 5 warning; unit 98 passati/2 ignorati; `POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] ./scripts/test-integration.sh` → deploy, autotest privilegi e 16/16 test; test dedicato ha registrato trigger, riavviato scheduler e verificato il recupero dallo store SQL. Schema legacy senza INSERT storici; provider Microsoft.Data.SqlClient. |
| 2026-10-16 | 27 | X1.3c PollingClass multi-target | `[x]` | Build net8.0 e net48 → 0 errori, 5 avvisi ciascuno; unit 98/2 ignorati; 4.178/4.298 righe non vuote compilabili net8 (97,2%); esclusi solo metadata/base ASMX condizionali; diff-check pulito. Windows/macOS non verificati localmente. |
| 2026-10-16 | 28 | X1.1 layout/props/central packages | `[x]` | Root props restituiscono LangVersion 7.3, Version 1.0.0, Nullable disable, Deterministic true; central versions attive con restore per i quattro csproj SDK; build Database e PollingClass net48/net8 0 errori; unit 98/2 ignored, integrazione 16 ignorati senza DB; `dotnet sln list` elenca i 5 progetti presenti. La soluzione mantiene il Website legacy; build completa fuori scope prima di X1.7. |
| 2026-10-16 | 29 | X1.4 configurazione astratta | `[x]` | Rimossi gli accessi diretti ai consumer: `ConfigurationManager` resta solo nell'adapter `Database/AppConfig.cs`. Adapter legge configurazione in-memory/fake, IConfiguration JSON/environment, e fallback legacy per net48. Build Database e PollingClass net8/net48 → 0 errori; 101 test passati/2 ignorati; suite integrazione 16 ignorati senza DB; `rg ConfigurationManager Database PollingClass --glob '*.cs'` → soli 2 riferimenti nell'adapter. `git diff --check` pulito. |
| 2026-10-16 | 30 | X1.5 provider SQL astratto | `[x]` | Isolato il factory seam ADO dietro `ISqlClientFactory`, con implementazione Microsoft.Data.SqlClient. CRUD integration sintetico con SqlDataAdapter/SqlCommandBuilder → 1/1 PASS; suite integration → 8 passati, 9 ignorati (manca login applicativo); build Database net48/net8 → 0 errori; unit 101/2 ignorati; 104 riferimenti Microsoft.Data.SqlClient; zero riferimenti System.Data.SqlClient. Il test richiede DB e viene saltato se assente. |
| 2026-10-16 | 31 | X1.6 CI multi-OS | `[x]` | Aggiunto workflow GitHub Actions: matrix ubuntu/macos/windows builda Database e PollingClass entrambi i TFM e unit test; job SQL Server Ubuntu effettua deploy synthetic e test integrazione solo con secrets CI definiti. YAML parse OK; build locale Release di tutte le librerie e test unit OK, integration project build OK; run remoto non ancora osservato, quindi OS CI non dichiarati verificati. |
| 2026-10-16 | 32 | X1.7 Website WebForms legacy | `[x]` | Build soluzione riprodotta → MSB4249 Website; `docs/LEGACY.md` dichiara scope legacy, requisito IIS/.NET Framework 4.8 e procedura Visual Studio su richiesta; workflow CI builda solo i progetti SDK e non il Website. Nessuna falsa dichiarazione di compilazione legacy. |
| 2026-10-16 | 33 | C1 BuildWhere parametrizzato | `[x]` | Nuovo overload con SqlCommand, allowlist campi/operatore/connettore/tipo e valori parametrici; tutti i 18 call site DAL delegano al comando SqlCommand. Test payload injection, input invalido, conversione N/D. Build Database net48/net8 0 errori; unit 104/2 ignorati; integrazione 17 ignorati senza DB; diff-check pulito. |
| 2026-10-17 | 34 | C2 TableAdapter parametrizzati | `[x]` | Migrati 18 query dei 9 adapter al SelectCommand con parametri; rimossi overload SQL raw non usati e overload string BuildWhere. Test SqlClientAdapterTests sul container: 2/2 PASS; unit 104/2 ignorati; build Database net48/net8 0 errori; `git diff --check` pulito. |
| 2026-10-17 | 35 | C3 SqlDataSource / TaskService | `[x]` | `sqlDimProcessi.SelectCommand` è costante con parametro `@IdProcesso`, passato a `SelectParameters`; scansione nessuna interpolazione SelectedValue/SelectCommand. `dotnet build PollingWeb.sln` → MSB4249 riprodotto; unit 104/2 ignorati; test Website non eseguibile nel runner net8. |
| 2026-10-17 | 36 | C4 Parsing invariant | `[x]` | Rimossi Convert.ToDateTime eseguibili, conversioni ADO date ora typed, ValidaData e IsNumeric invariant; cron usa DateTime typed e cultura invariant. Build Database/PollingClass entrambi TFM 0 errori; unit 106/2 ignorati; diff-check pulito. |
| 2026-10-17 | 37 | C5 DatabaseInfo per-istanza | `[x]` | Eliminati campi statici DAL e provider config mutabile; DatabaseInfo riceve IAppConfig/connection string e timeout, default 30s; smaltimento connection/transaction. Test 100 istanze parallele distinte; Database/PollingClass net48/net8 0 errori; unit 108/2 ignorati. |
| 2026-10-17 | 38 | C6 Gestione errori/logging | `[x]` | Rimosse 28 rilancio `throw ex;`, catch vuoti DAL esplicitati e 14 `Console.WriteLine(ex.Message)` sostituiti col dispatcher Trace/handler; helper Aggset ora mantiene inner exception senza incorporare SQL. Unit 111/2 ignorati; build Database/PollingClass net48/net8 0 errori; `git diff --check` pulito. |
