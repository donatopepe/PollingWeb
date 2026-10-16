# LOOP LOG — PollingWeb remediation

Registro append-only delle iterazioni del loop.

Formato riga:
`## <timestamp UTC> — <TASK ID> <titolo> — <stato>`
poi: contesto, modifiche, evidenza (comando + output), prossimo task, blocchi.

---

## 2026-xx-xx T00 — Studio iniziale e pianificazione — DONE

**Contesto.** Repository `github.com/donatopepe/PollingWeb` (branch `master`, singolo
commit `6b01bfa` del 2023-01-16). Soluzione .NET Framework 4.5: `Database`,
`PollingClass`, `PollingWeb` (Website project) + due database project SQL
(`DatabaseSQL`, `Polling`).

**Cosa è stato accertato (dettaglio in `PLAN.md` §2).**

- `Database` e `PollingClass` compilano su net48 con dotnet 8 + reference assemblies
  (`/tmp/pb`): 0 errori, 5 warning (variabili inutilizzate).
- `PollingClass/Check_RdA.cs` è codice VB→C# rotto e orfano: unico errore di build
  (`CS1525` a 82,53). Escludendolo la build passa.
- `PollingWeb` è un Website project con `CodeFile=` e classi `partial` senza
  dichiarazioni dei controlli → non compilabile senza generare `.designer.cs`.
- Credenziali reali hardcoded, nella storia Git di una repo privata: `Web.config`, `Database/app.config`,
  `Database/Properties/Settings.Designer.cs`, `Database/bin/*/Database.dll.config`.
- Dati reali nel repository: `script_eprocurement_donato.sql` (15.732 `INSERT`),
  `SC00G100_inizio.TXT` (anagrafiche fornitori + P.IVA), `BDG_MCC.xlsx`.
- SQL injection in `Database/DataType.cs` (`FiltroCampi.BuildWhere`, concatenazione) e
  in `PollingWeb/TaskService.aspx.cs` (`SelectCommand` concatenato).
- Nessun `.gitignore`: `bin/`, `obj/`, `packages/` (68 MB), `.vs/` tracciati
  (123 file su 382).
- Nessun README, LICENSE, test, CI.

**Deliverable prodotti.**

- `PLAN.md` — piano con 47 task, DoD, dipendenze, rischi, 11 criteri di accettazione.
- `PROMPT.md` — prompt ingegnerizzato con vincoli duri e algoritmo di iterazione.
- `docs/LOOP_LOG.md` — questo registro.
- Loop cron creato per l'esecuzione incrementale.

**Evidenza.**

```
$ /opt/dotnet/dotnet build   # /tmp/pb/PollingClass, net48
Compilazione completata.  0 errori, 5 warning
$ git ls-remote origin
6b01bfa...  refs/heads/master
```

**Prossimo.** `T0.1` — inventario segreti in `docs/SECURITY-FINDINGS.md`.

**Blocchi.** Nessuno. Nota: **correzione successiva** — a questo punto avevo scritto che la
repository era pubblica. Verificato in `T0.10` con `gh repo view`: è **privata**, unico
collaboratore il proprietario, 0 fork. I segreti non sono stati esposti pubblicamente; la
rotazione resta comunque obbligatoria prima della pubblicazione (debolezza intrinseca,
copie non verificabili, pubblicazione imminente).

---

## 2026-10-16T09:4xZ — T0.1 Inventario segreti — DONE

**Contesto.** Serve una fotografia completa e classificata di segreti e dati sensibili prima
di qualunque intervento di sanificazione, per non dimenticare occorrenze e per poter misurare
l'azzeramento (`T0.2`–`T0.4`).

**Modifiche.**
- `docs/SECURITY-FINDINGS.md` (nuovo) — 8 classi di finding (S1–S8) con gravità, file:riga,
  comandi di riproduzione, soglia di superamento e piano di rotazione credenziali.

**Evidenza (comandi eseguiti e output).**

> Nei comandi seguenti `$PATTERNS` è l'alternanza dei valori sensibili, letta dal record di
> audit offline: `PATTERNS="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"`.
> I valori non sono riportati in questo registro (vedi `docs/RELEASING.md`).

```
$ grep -rn "WITH PASSWORD" DatabaseSQL/Security PollingDB/Security | wc -l
2
$ grep -rn "$PATTERNS" PollingWeb/Web.config Database/app.config \
    Database/Properties/Settings.Designer.cs Database/Properties/Settings.settings \
    Database/bin PollingClass/bin PollingWeb/Bin | wc -l
22
$ grep -rn "<legacy-login>" --include=*.cs . | wc -l
1
$ grep -rnoE "$PATTERNS" $(git ls-files | grep -v DocumentFormat.OpenXml.xml) | wc -l
87
$ grep -c "INSERT" script_eprocurement_donato.sql
15732
```

**Finding rilevanti emersi.**
- S1: due `CREATE LOGIN` con password in chiaro, `CHECK_POLICY = OFF` e `SID` esplicito
  (`DatabaseSQL/Security/<legacy-login>_1.sql`, `PollingDB/Security/<legacy-login>_1.sql`).
- S3: coppia `<legacy-login>` / `<REDACTED>` su un host interno in commento in `ParserFatture.cs:186`.
- S4: 58 delle 87 occorrenze sono in `*/obj/` (artefatti) → il `.gitignore` di `T0.5` ne
  rimuove la maggior parte dal versionamento.
- S6: `MailSede` non autentica realmente l'SMTP (credenziali placeholder) → non è un leak,
  ma un difetto funzionale da correggere in fase 2.

**Prossimo.** `T0.5` — `.gitignore` + `.gitattributes` (prerequisito che riduce subito S4).

**Blocchi.** Nessuno.

## 2026-10-16T10:0xZ — T0.5 .gitignore + .gitattributes — DONE

**Contesto.** 148 artefatti di build (`.vs/`, `*/bin/`, `*/obj/`, `packages/`) e 6 file
utente (`*.user`, `*.suo`) erano versionati; 58 delle 87 occorrenze S4 (hostname interni)
venivano proprio da `*/obj/*FileListAbsolute.txt`. Nessuna regola di normalizzazione dei
line ending.

**Verifica preliminare dei line ending** (per non introdurre una renormalizzazione di massa):

```
$ git ls-files --eol | awk '{print $1, $2}' | sort | uniq -c
    268 i/lf w/lf      # sorgenti già LF in index e working tree
    105 i/-text w/-text # rilevati binari (UTF-16: i 3 dump SQL)
     13 i/none w/none   # vuoti
```

Nessun file sorgente era CRLF → `* text=auto` non produce renormalizzazione.

**Modifiche.**
- `.gitignore` (nuovo) — build output, NuGet, Visual Studio, SQL project, config locali,
  dati/log, stato del loop (`.pi-loop.json`), OS/editor.
- `.gitattributes` (nuovo) — `* text=auto`; forzatura a `text` per `.sln`/`.csproj`/`.sqlproj`/
  `.props`/`.targets` e per configurazioni XML (contengono BOM UTF-8 e non devono essere
  mai classificati binari); `binary` esplicito per DLL/immagini/archivi; `-text` di guardia
  per i 3 dump SQL UTF-16 ancora presenti fino a `T0.3`.
- De-tracking (senza cancellare dal disco) di `.vs/`, `Database/bin`, `Database/obj`,
  `PollingClass/bin`, `PollingClass/obj`, `PollingWeb/Bin`, `DatabaseSQL/obj`, `packages/`
  e dei 6 file utente.

**Evidenza.**

```
$ git ls-files | grep -cE '^(\.vs/|.*/[Bb]in/|.*/[Oo]bj/|packages/)'
0
$ git ls-files | wc -l
234                     # era 382 → -148 artefatti
$ mkdir -p obj/Simulazione bin/Simulazione && touch obj/Simulazione/gen.cs bin/Simulazione/App.dll Database/Foo.csproj.user
$ git check-ignore -v obj/Simulazione/gen.cs bin/Simulazione/App.dll Database/Foo.csproj.user
.gitignore:10:**/[Oo]bj/   obj/Simulazione/gen.cs
.gitignore:9:**/[Bb]in/    bin/Simulazione/App.dll
.gitignore:31:*.user       Database/Foo.csproj.user
$ PATH=/opt/dotnet:$PATH dotnet build Database/Database.csproj
error MSB3644: gli assembly di riferimento per .NETFramework,Version=v4.5 non sono stati trovati  # atteso: risolto da T1.1
$ git ls-files --others --exclude-standard
.gitattributes
.gitignore              # nessun artefatto di build fra i non tracciati → OK
```

Il fallimento `MSB3644` è **atteso** e non è imputabile a T0.5: la conversione a `net48`
con `Microsoft.NETFramework.ReferenceAssemblies` è il task `T1.1`.

**Prossimo.** `T0.2` — esternalizzazione della configurazione (azzera S2 e S4 nei sorgenti).

**Blocchi.** Nessuno. Nota per `T0.3`: `bin/`, `obj/`, `packages/`, `.vs/`, `*.user` sono
già de-tracciati; restano da rimuovere i dati reali (S5) e la cartella `Importa log di schema/`.

## 2026-10-16T10:15Z — REV.2 Piano: track cross-platform — DONE (task di revisione)

**Contesto.** Nuovo requisito esplicito: il progetto deve essere **cross-platform**. La
revisione 1 del piano vincolava il runtime ad ASP.NET WebForms su .NET Framework 4.8 / IIS,
quindi obiettivo, vincoli e fasi andavano riscritti, non solo estesi.

**Analisi di portabilità eseguita prima di decidere** (dettaglio in `PLAN.md` §2.2):

```
$ grep -rn "HttpContext\.Current\|HostingEnvironment\|MapPath\|BuildManager" --include=*.cs Database PollingClass
(nessun risultato)                        -> P1: dominio già disaccoppiato dal web

$ grep -rn "System\.Windows\|Dispatcher\|DependencyObject" --include=*.cs Database PollingClass
(nessun risultato)                        -> P5: WindowsBase in PollingClass.csproj è inutile

$ grep -rn "class Colore\|FontSerif\|Aggset\.Color" --include=*.cs
Database/DataType.cs: class Colore / class FontSerif
PollingClass/Check_RdA.cs                -> P6: System.Drawing usato SOLO dal file rotto da eliminare

$ grep -rn "System.Web.Services.WebService" --include=*.cs PollingClass
Import_Budget.cs Ordini_Sap.cs LeggiFornitore.cs ImportFatture.cs
                                          -> P3: unico blocco reale (ASMX non esiste su .NET 8)
```

**Decisione architetturale.**
- Track A (prioritario): librerie SDK-style multi-target `net48;net8.0` → `PollingWorker`
  (.NET 8 + Quartz 3.x) + `PollingApi` (ASP.NET Core al posto dei 4 ASMX) → il **motore** gira
  su Linux/macOS/Windows senza IIS.
- Track B (Fase 4): UI portata a Razor Pages (`PollingWeb.Core`) per la parità completa.
- Legacy: `net48`/IIS conservato come *legacy mode* documentata (`docs/LEGACY.md`), non bloccante.

**Modifiche.**
- `PLAN.md` riscritto: revisione 2, obiettivo cross-platform, vincolo n.4 sostituito, nuovo
  §2.2 (analisi di portabilità P1–P12) con decisione architetturale e diagramma, fasi
  ristrutturate in **X1–X4** (cross-platform), **C** (correttezza), **T** (test), **D** (docs),
  **R** (release); **64 task**, **15 criteri di accettazione** (AC3/AC4/AC10/AC13 nuovi e
  specifici per il cross-platform); ordine di esecuzione aggiornato; mappatura T1.x → X1.x.
- `PROMPT.md`: ruolo e obiettivo aggiornati, vincoli rinumerati (il divieto di .NET Core è
  diventato "il port cross-platform è la direzione"), aggiunto l'obbligo di verificare entrambi
  i TFM e il divieto di dichiarare verificato ciò che non è stato eseguito.
- Loop cron ricreato (vecchio ID `599076ee` cancellato → nuovo ID `a6950558`) perché il
  vecchio prompt citava AC1–AC11.

**Evidenza.**

```
$ grep -oE '\*\*(T0\.[0-9]+|X[0-9]\.[0-9]+|C[0-9]+|T[0-9]+|D[0-9]+|R[0-9]+)\b' PLAN.md | sort -u | wc -l
64
$ grep -cE '^- \[ \] \*\*AC' PLAN.md   # criteri di accettazione
15
$ grep -cE '^- \[x\]' PLAN.md            # task gia' verificati
2
$ cron_list
[a6950558] every 15 minutes (*/15 * * * *)
  Next: 16/10/2026, 10:30:00 | recurring, durable
```

**Nota vincolante registrata nel piano.** Il loop non deve dichiarare verificati macOS e
Windows: non sono disponibili in questo ambiente. Per quei task l'unico evidence ammesso è la
CI multi-OS (X1.6). Il limite è dichiarato in `PLAN.md` §5 e in `PROMPT.md` vincolo 9.

**Prossimo.** `T0.2` — esternalizzazione configurazione (azzera S2 e S4 nei sorgenti e redige
gli hostname interni citati in `PLAN.md`/`PROMPT.md`/`docs/LOOP_LOG.md`).

**Blocchi.** Nessuno.

## 2026-10-16T10:30Z — T0.2 Esternalizzazione configurazione — DONE

**Contesto.** Credenziali DB, hostname interni e URL TFS erano hardcoded in 20 file tracciati
(S2, S3, S4 di `docs/SECURITY-FINDINGS.md`). Il repository diventa pubblico: nessun file
versionato deve contenere valori reali.

**Riproduzione iniziale.**

```
$ git grep -El "$PATTERNS"   # PATTERNS dal record di audit offline
20 file (incl. 2 binari *.dbmdl, PollingWeb.sln, Security/*.sql, Settings.*)
```

**Modifiche.**

| File | Intervento |
|------|-----------|
| `PollingWeb/Web.config` | placeholder `${POLLING_DB_SERVER}` / `${POLLING_DB_USER}` / `${POLLING_DB_PASSWORD}` / `${POLLING_SMTP_HOST}`; header esplicativo |
| `PollingWeb/Web.config.example` | **nuovo** — modello commentato con il significato di ogni chiave |
| `Database/app.config` | placeholder; commento sul naming `Database.Properties.Settings.*` |
| `Database/app.config.example` | **nuovo** — modello commentato |
| `Database/Properties/Settings.Designer.cs` | `DefaultSettingValueAttribute("")` per entrambe le impostazioni |
| `Database/Properties/Settings.settings` | `ConnectionString` vuota e `<Value>` vuoto |
| `PollingWeb.sln` | rimossa la sezione `TeamFoundationVersionControl` e le righe `Scc*` (URL TFS interno) |
| `PollingClass/ParserFatture.cs` | rimossa la riga commentata con utente/password legacy (S3) |
| `PollingClass/Check_RdA.cs` | letterale IP sostituito con `<DB_HOST>` (file comunque rimosso in T0.6) |
| `PollingWeb/App_Data/PublishProfiles/FolderProfile.pubxml` | `D:\<path-interno>` → `publish\PollingWeb` (S7) |
| `*/Security/<legacy-login>_1.sql` → `01-create-app-login.sql` | **rinominato**; password da variabile sqlcmd; `CHECK_POLICY = ON`; SID esplicito rimosso |
| `*/Security/<legacy-login>.sql` → `02-create-app-user.sql` | **rinominato**; login da variabile sqlcmd; idempotente |
| `*/Security/Permissions.sql` | `GRANT CONNECT TO [$(PollingAppLogin)]` |
| `PollingDB.sqlproj` | percorsi `Build Include` aggiornati + `SqlCmdVariable` `PollingAppLogin` / `PollingAppPassword` (password senza default → deploy fail-closed) |
| `DatabaseSQL/*.dbmdl`, `PollingDB/*.dbmdl` | **de-tracciati** (binari, coperti da `.gitignore`) |
| `PLAN.md`, `PROMPT.md`, `docs/LOOP_LOG.md` | hostname/credenziali redatti con placeholder; i valori sono sostituiti da placeholder; le evidenze leggono i pattern dal record di audit offline |

**Hardening contestuale sulla sicurezza (S1).** `CHECK_POLICY = OFF` → `ON`, `SID` esplicito
rimosso, password non più nel repository: è la mitigazione di S1, non un refactor.

**Bug reale trovato dal build.** Il primo `Database/app.config` conteneva righe separatore
`-----------------` dentro un commento XML: `MSB3249: An XML comment cannot contain '--'`.
Rilevato dal build, non a occhio. Corretto e poi validati tutti i file XML toccati con un
parser reale (`xml.etree.ElementTree`).

**Evidenza.**

```
$ git grep -nE "$PATTERNS" -- . ':(exclude)docs/SECURITY-FINDINGS.md'
(nessun output) ; exit=1                      # 0 risultati fuori dall'inventario

$ git grep -cE "$PATTERNS" -- docs/SECURITY-FINDINGS.md
0
```

> Dopo `T0.10` la stessa ricerca dà 0 risultati anche **dentro** `docs/SECURITY-FINDINGS.md`:
> l'inventario è stato redatto e i 21 riferimenti sono diventati placeholder.

```
$ dotnet build Database/Database.sdk.csproj        -f net48   # harness /tmp/t02
Compilazione completata.  Avvisi: 0   Errori: 0
$ dotnet build PollingClass/PollingClass.sdk.csproj -f net48
Compilazione completata.  Avvisi: 5   Errori: 0

$ python3 -c "import xml.etree.ElementTree as ET; ET.parse('<file>')"   # su Web.config,
  Web.config.example, app.config, app.config.example, Settings.settings, PollingDB.sqlproj
  FolderProfile.pubxml                        # tutti validi (PollingWeb.sln non e' XML)

$ grep -c 'GlobalSection(' PollingWeb.sln / -c 'EndGlobalSection'   -> 4 / 4
$ grep -c '^Project(' PollingWeb.sln      / -c '^EndProject$'       -> 3 / 3
$ grep -c 'Scc' PollingWeb.sln                                       -> 0

$ git ls-files | wc -l
234          # -2 (*.dbmdl de-tracciati), +6 (nuovi file/sostituzioni)
```

Coerenza chiavi: il codice legge `ConnectionStrings["PollingConnectionString"]`,
`ConnectionStrings["PollingEprocurement"]`, `AppSettings["smtpServer"]`; tutte presenti in
`Web.config` con i nomi invariati → nessuna rottura a runtime.

**Non incluso (deferito, non dimenticato).**

- `providerName="System.Data.EntityClient"` su stringhe SqlClient (F14) → **C7**.
- `debug="true"` e `customErrors mode="Off"` lasciati come nell'originale → **C7**
  (cambiare il comportamento a runtime non è compito dello svuotamento dei segreti).
- Risoluzione automatica dei `${...}` da variabili d'ambiente → **X1.4** (configurazione
  astratta). Oggi i placeholder vanno sostituiti al deploy.
- Hardening completo dei permessi minimi → **T0.8**.

**Prossimo.** `T0.3` — rimozione dei dati reali e creazione di `samples/` sintetici.

**Blocchi.** Nessuno.

## 2026-10-16T10:45Z — T0.3 Rimozione dati reali + samples/ — DONE

**Contesto.** Il repository conteneva 14,3 MB di dati di terzi: due dump con **15.732
istruzioni `INSERT`** di produzione, l'anagrafica fornitori reale con P.IVA e codici fiscali,
un'estrazione di budget, due tracciati SAP reali e due log di import schema con nomi di
macchine e utenti di sviluppo. Questi file sono incompatibili con la pubblicazione.

**Scoperta rilevante (schema drift) — il motivo per cui non ho cancellato il dump e basta.**

Prima di rimuovere `script_polling.sql` ho confrontato gli oggetti che contiene con quelli
presenti nei progetti SSDT:

```
$ comm -23 <oggetti nel dump> <oggetti nel progetto>
Flusso_Ordini_SAP_old
RDA_Esiti_LastOutput_OnLine
Tbl_Export_SAP_OnLine
```

Due di questi sono **usati dal codice applicativo**
(`Database/TableAdapterRDA_Esiti_LastOutput_OnLine.cs`, `Database/TableAdapterTbl_Export_SAP_OnLine.cs`,
`PollingClass/Ordini_Sap.cs`, `PollingClass/Import_Budget.cs`) ma **non esistevano in nessun
progetto SQL**: cancellare il dump avrebbe perso la loro DDL per sempre. Estratte quindi in
`PollingDB/dbo/Tables/RDA_Esiti_LastOutput_OnLine.sql` e `Tbl_Export_SAP_OnLine.sql`
(+ copia in `DatabaseSQL/`, in attesa del consolidamento di `T0.7`), con le colonne verificate
una per una contro il dump. `Flusso_Ordini_SAP_old` è una stored procedure obsoleta e non
referenziata: non ripristinata, conservata nel record di audit offline.

**Modifiche.**

| Intervento | Dettaglio |
|---|---|
| Rimossi | `script_eprocurement_donato.sql` (12,7 MB), `script_polling_donato.sql` (1,3 MB), `SC00G100_inizio.TXT` (254 KB), `BDG_MCC.xlsx`, `_047_1047_oda_input.#00`, `_063_1063_rda_input.#00`, `*/Importa log di schema/*.log`, `script_polling.sql` (3,3 MB) |
| Recuperati | 2 tabelle mancanti → `DatabaseSQL/` e `PollingDB/dbo/Tables/`, con `<Build Include>` nel `PollingDB.sqlproj` |
| Nuovi campioni | `samples/README.md`, `samples/sap/{README.md,oda_esiti_input.sample,rda_input.sample,SC00G100.sample.txt}`, `samples/budget/{README.md,BDG_sample.xlsx}`, `samples/tools/make-budget-sample.py`, `samples/seed/seed.sql` |

**Verifiche eseguite sui campioni (non solo "creati").**

Il campione XLSX è generato da script (deterministico, nessun binario opaco) e **aperto con la
stessa libreria del codice applicativo**, replicando la logica di `ReadExcelFileDOM`:

```
[IST][CDR][VOCE][BDGRILASCI][BDGRESIDUO][CODFONDO][DATARIF]
[1][DEMO01][V0001][1000.00][1000.00][FONDO-A][2024-01-01]
OK righe=5 celle=35 sharedStringsIrrisolte=0
```

`sharedStringsIrrisolte=0` dimostra che la `SharedStringTable` è corretta: senza di essa il
parser produrrebbe stringhe vuote. Validati anche tutti i membri XML dello zip.

Il campione `SC00G100.sample.txt` è stato costruito **dal codice di lettura**, non a
intuizione: `LeggiFornitore.cs` mappa `values[1]=Cfor, [2]=Ragione_Sociale, …, [7]=Piva|Codfis`,
accetta 9 campi con `|` iniziale e 8 senza (anteponendolo). Il campione contiene 4 righe a
9 campi e 1 a 8 campi. Verificato con `len(line.split('|'))`.

**Evidenza.**

```
$ git ls-files -z | xargs -0 du -cb | tail -1        # contenuto tracciato
2508750        # era 5854377 -> -3,3 MB
$ du -sh --exclude=.git --exclude=packages --exclude=.vs --exclude=bin --exclude=obj .
3,2M           # < 5 MB, obiettivo della DoD
$ grep -c INSERT script_eprocurement_donato.sql      # prima
15732          # ora il file non esiste
$ python3 -c "import xml.etree.ElementTree as ET; ET.parse('PollingDB/PollingDB.sqlproj')"
(nessun errore)
$ grep -rhoE '[0-9]{11,17}' samples/ | sort -u
00000000000000000 00000000001 00000000002 00000000003 00000000004 00000000005
```

**Non verificato — dichiarato.** `samples/seed/seed.sql` **non è stato eseguito** su un'istanza
SQL Server: non era disponibile in questa iterazione. Sintassi e conteggio colonne dei `MERGE`
sono stati controllati a mano (14=14 per `TaskService`, 9=9 per `TargetSC00G100`, cast espliciti
sui tipi, `WITH (HOLDLOCK)` su tutti). L'esecuzione reale è demandata a `T0.8`/`T3.1`, che
introducono il container SQL Server. Il dialetto `³` del tracciato fornitori non è coperto dai
campioni (richiede un file in code page 1252): lacuna dichiarata in `samples/sap/README.md`.

**Prossimo.** `T0.4` — de-branding; poi `T0.10` (riscrittura storia), richiesto esplicitamente
dall'utente e ora eseguibile perché dati e credenziali sono fuori dal tree.

**Blocchi.** Nessuno.

## 2026-10-16T11:00Z — T0.6 Pulizia file morti — DONE

**Contesto.** Il repository conteneva file non compilati, duplicati obsoleti e — scoperta
durante il lavoro — **20 asset di terze parti non ridistribuibili**.

**Trovato durante l'ispezione.** I 20 file `PollingWeb/js/1x_warning.htm` … `3x_warning.htm`
non sono JavaScript: sono **pagine di blocco di McAfee Web Gateway**, salvate al posto dei
download originali durante lo sviluppo (contengono il logo McAfee e "A file has been blocked
by a file-filtering rule"). Non sono licenziate per la redistribuzione e non hanno alcuna
funzione applicativa: nessun file le referenzia.

**Modifiche.** 53 file rimossi (239 → 186 tracciati).

| Categoria | File |
|---|---|
| Codice rotto | `PollingClass/Check_RdA.cs` (conversione VB→C# incompleta, fuori dal `.csproj`, unico errore di build se incluso) |
| Duplicati obsoleti | 24 × `PollingWeb/App_Code/*.exclude` |
| CSS duplicato | `PollingWeb/css/style - Copia.css` |
| Asset di terzi | 20 × `PollingWeb/js/*_warning.htm` (pagine di blocco McAfee) |
| Placeholder vuoti | `Procedure1.sql`, `Procedure2.sql`, `Script1.sql`, `Script2.sql` |
| Codice irraggiungibile | `PollingWeb/App_Code/TEST_QUARTZ.cs` (web service senza `.asmx`) |
| Template non usato | `PollingWeb/job_scheduling_data_2_0.xsd` |
| Ricodifica | `sript_quartz.sql` (UTF-16, nome con refuso) → **`sql/quartz-schema.sql`** (UTF-8, con header di provenienza) |

**Verifica preliminare: nessuno era referenziato.** Prima di rimuovere, per ciascuno:

```
$ git grep -l "_warning"      -- . | grep -v '_warning.htm'   -> (nessuno)
$ git grep -l "TEST_QUARTZ"   -- . | grep -v 'App_Code/'      -> (nessuno; nessun .asmx)
$ git grep -l "style - Copia" -- .                            -> (solo PLAN.md)
$ git grep -l "job_scheduling_data_2_0" -- .                  -> (nessuno)
$ grep -n "Check_RdA\|Script1\|Script2\|Procedure1\|Procedure2" *.csproj *.sqlproj  -> (nessuno)
```

**Evidenza.**

```
$ git ls-files | wc -l
186                        # era 239
$ grep -c "CREATE TABLE" sql/quartz-schema.sql
11                         # tabelle QRTZ_* conservate
$ file sql/quartz-schema.sql
UTF-8 (with BOM)           # era UTF-16
$ git grep -nE 'Check_RdA|TEST_QUARTZ|job_scheduling_data_2_0|style - Copia|Procedure[12]|Script[12]\.sql|_warning\.htm|sript_quartz' \
      -- . ':(exclude)PLAN.md' ':(exclude)PROMPT.md' ':(exclude)docs/LOOP_LOG.md'
(nessun output) ; exit=1
$ dotnet build Database.sdk.csproj     -f net48   -> 0 errori
$ dotnet build PollingClass.sdk.csproj -f net48   -> 0 errori
```

**Perché l'evidenza esclude i documenti di lavoro.** `PLAN.md`, `PROMPT.md` e
`docs/LOOP_LOG.md` devono necessariamente nominare ciò che è stato rimosso: un `git grep`
"= 0" su tutto l'albero sarebbe impossibile da soddisfare e quindi una falsa verifica. La
verifica è quindi esplicita sui file **diversi** dai documenti di lavoro. Se in futuro si
deciderà di pubblicarli (decisione demandata a `R6`), andranno redatti.

**Nota su `sql/`.** La struttura è provvisoria: lo schema applicativo è ancora duplicato in
`DatabaseSQL/` (il cui `.sqlproj` non include nulla: file orfani) e `PollingDB/` (l'unico che
compila). Il consolidamento è `T0.7`. Descritto in `sql/README.md` perché è esattamente il
tipo di ambiguità che ha quasi fatto perdere due tabelle in `T0.3`.

**Prossimo.** `T0.11` — rimozione degli identificatori derivati dall'azienda (`PollingException*`
168 occorrenze, `Polling*`, directory e progetto SQL), introdotto dopo la richiesta
esplicita dell'utente: il progetto pubblico non deve contenere alcun riferimento all'azienda,
nemmeno negli identificatori di codice.

**Blocchi.** Nessuno.

## 2026-10-16T11:05Z — T0.4 De-branding — DONE

**Contesto.** Il progetto portava il marchio dell'istituto per cui era stato sviluppato: logo
registrato, nome nel testo del template email, email aziendali e personali hardcoded, path di
sviluppatore con nome utente.

**Modifiche.**
- `PollingWeb/images/<bank-logo>.png` **rimosso** (marchio registrato di terzi, non
  ridistribuibile con licenza MIT) → nuovo `PollingWeb/images/logo.svg`, segnaposto neutro
  rilasciato con la stessa licenza del progetto.
- `Polling_web.master`: `src` aggiornato a `logo.svg`, `alt="PollingWeb"`, `<title>` da
  `<acronimo istituto> - Admin Panel` a `PollingWeb - Admin Panel`.
- `PollingClass/MailSede.cs`: rimosso dal corpo delle email il `<img>` che puntava a
  `http://www.<dominio-istituto>/.../logo_*.gif` — un asset di terzi servito da un dominio
  esterno.
- `PollingClass/ParserFatture.cs`, `PollingClass/Check_RdA.cs`: rimosse email aziendali
  (`EProcurement@…`) e **dati personali** (indirizzo email di una persona) usati come
  destinatari hardcoded.
- `PollingClass/ImportaXML.cs`, `PollingWeb/Default.aspx.cs`: rimossi path assoluti con
  nome utente di sviluppatore (`C:\Users\<nome>\Desktop\...`) e riferimenti al progetto
  personale.
- **`NOTICE`** (nuovo): attribuzione storica **generica** (nessun nome dell'istituto), marchi
  di terzi dichiarati come tali, inventario delle licenze con evidenza della dipendenza
  copyleft da rimuovere (`X1.3`), e la nota che il file è provvisorio fino a `D10`.
- **`docs/RELEASING.md`** (nuovo): record di audit, procedura di riscrittura della storia,
  checklist di pubblicazione, versioning.

**Evidenza.**

```
$ PAT="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"
$ git grep -ilE "$PAT" -- . ':(exclude)NOTICE'
(nessun output) ; exit=1
```

Il pattern è composto a runtime dal record di audit: **il nome dell'istituto, il suo dominio
e il nome del logo non compaiono in nessun file del repository**, inclusi i documenti che
descrivono la rimozione. `NOTICE` è l'unica eccezione ammessa: contiene l'attribuzione
storica senza nominare l'istituto.

**Nota metodologica.** Anche la riga di evidenza di `PLAN.md` è stata riscritta per leggere i
pattern dal record di audit invece di contenerli: un'evidenza che si auto-soddisfa (o che si
auto-fallisce) non è un'evidenza.

**Prossimo.** `T0.11` — identificatori derivati dall'azienda.

**Blocchi.** Nessuno.

## 2026-10-16T11:20Z — T0.11 Identificatori aziendali — DONE

**Contesto.** Richiesta esplicita dell'utente: nel progetto pubblico non deve esistere alcun
riferimento all'azienda, **nemmeno negli identificatori di codice**. L'abbreviazione compare
168 volte nel framework di eccezioni, 42 in `Polling*` (database, chiavi di connessione,
directory di progetto), più variabili e riferimenti in documenti e pagine.

**Modifiche.** Rinominati (mappatura completa nel record di audit offline):

| Categoria | Da → A |
|---|---|
| Framework eccezioni | 4 tipi con prefisso dell'azienda → `PollingException`, `PollingExceptionType`, `PollingExceptionHandler`, `GestisciPollingException`; file `Database/PollingException.cs` |
| Chiavi di connessione | chiave senza suffisso → `PollingConnectionString`; variante `DB` → `PollingConnectionStringDirect` |
| Database | nome del database applicativo → `Polling` |
| Progetto SQL | directory → `PollingDB/`, progetto → `PollingDB.sqlproj` (`Name`, `RootNamespace`, `AssemblyName`) |
| Variabili | 2 variabili interne con l'abbreviazione → `dbInfoPolling`, `DemoPolling` |

La mappatura completa **vecchio → nuovo** e' nel record di audit offline (`REDACTED-VALUES.md`): i vecchi identificatori non compaiono in questo
repository, perche' sono essi stessi il riferimento all'azienda che il
task rimuove.

Contestualmente corretti i 4 `.asmx`: l'attributo `CodeBehind="~/App_Code/*.cs"` puntava a file
inesistenti (erano già `.exclude`, e in `T0.6` la cartella è stata rimossa). Rimosso
l'attributo: la classe si risolve dall'assembly `PollingClass.dll` in `Bin`, come previsto dal
progetto Website.

**Metodo.** Rinominazione con `git ls-files -z | xargs -0 sed` (a prova di spazi nei
percorsi — un primo tentativo con `$(...)` non quotato si è rotto su
`dbo/Stored Procedures/*.sql` ed è stato rifatto). Ordine delle sostituzioni scelto per
evitare collisioni di prefisso: la chiave con suffisso `DB` prima di quella senza, il
file di progetto prima del nome del database, i tipi di eccezione piu' specifici prima
di quello generico.

**Attenzione a una falsa pista.** `git grep -i popolare` segnala 18 occorrenze di
"METODI PER POPOLARE IL DATASET": è l'italiano *popolare* = riempire, non il nome
dell'istituto. Nessun intervento.

**Evidenza.**

```
$ git grep -c "$OLD_IDENTIFIERS" -- .   -> 0 occorrenze
      # OLD_IDENTIFIERS = alternanza dei vecchi identificatori, dal record di audit
$ PAT="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"
$ git grep -inE "$PAT" -- . ':(exclude)NOTICE'   -> exit 1 (0 risultati)

# coerenza fornitori/consumatori della chiave di connessione
definizioni (.config/.settings/.Designer.cs/.xsd)  -> 13
consumatori .aspx (SqlDataSource)                  -> 13
consumatori .cs                                    -> 4
$ git grep -c "eProcurementConnectionString" -- ...
  Settings.Designer.cs:3  Settings.settings:1  app.config:1  dtsTabelle.xsd:1   # invariata

# classi di eccezione
classi dichiarate: 2    file che le usano: 15

$ dotnet build Database.sdk.csproj     -f net48 -> 0 errori
$ dotnet build PollingClass.sdk.csproj -f net48 -> 0 errori
$ python3 -c "ET.parse(...)" su 4 file di progetto -> XML validi
```

**Limite dichiarato.** I riferimenti negli `.aspx` non sono verificabili con un build finché
il progetto web non è compilabile (`X1.7`/`X4.2`). La verifica è il **grep incrociato**
(13 definizioni / 13 consumatori): forte, ma non equivalente a una compilazione. Va
confermata quando il progetto web entrerà in build.

**Conseguenza operativa da documentare.** Il nome del database applicativo è cambiato: un
ambiente esistente richiede `ALTER DATABASE ... MODIFY NAME` o una nuova istanza. Va scritto
in `CHANGELOG.md` e in `docs/INSTALL.md` (entrambi previsti nei task `D1`/`D2`), e in
`scripts/db-deploy.sh` (`T0.8`) che dovrà creare `Polling`.

**Prossimo.** `T0.10` — riscrittura della storia Git, ora eseguibile: credenziali, dati,
marchio e identificatori sono fuori dall'albero.

**Blocchi.** Nessuno.

## 2026-10-16T11:40Z — T0.10 Riscrittura della storia Git — DONE

**Contesto.** Richiesta esplicita dell'utente. Il commit radice (2023-01-16, repository
**privata** — verificato con `gh repo view`: `visibility: private`, unico collaboratore il
proprietario, 0 fork) conteneva credenziali di produzione, dati reali di terzi e
riferimenti all'istituto. Dopo
`T0.2`–`T0.11` l'albero non contiene più nulla di tutto ciò: ora si poteva ricostruire la
storia senza trascinarsi dietro la versione precedente.

**Eseguito in anticipo rispetto a §4 del piano** (che collocava `T0.10` per ultimo) su
richiesta dell'utente. Il loop schedulato è stato **messo in pausa** prima di iniziare
(`cron_delete`): riscrivere la storia mentre un processo committa è una corsa garantita.

**Procedura.**

```
# 1. backup verificabile, FUORI dal repository
git bundle create "$AUDIT_DIR/full-history.bundle" --all   # 9 commit
git bundle verify "$AUDIT_DIR/full-history.bundle"         # "cronologia completa"
sha256sum full-history.bundle > full-history.bundle.sha256

# 2. storia nuova
git checkout --orphan main
git add -A                     # 190 file, nessun artefatto ignorato
# controllo pre-commit: pattern dal record di audit -> 0 risultati
git commit -m "chore: pubblica PollingWeb su storia Git ripulita [T0.10]"

# 3. eliminazione di TUTTO ciò che puntava alla vecchia storia
git branch -D master
git for-each-ref --format='%(refname)' refs/remotes/ | xargs -n1 git update-ref -d
rm -f .git/refs/remotes/origin/HEAD
git reflog expire --expire=now --all
git gc --prune=now
```

**Errore intercettato al primo tentativo — e perché conta.** Dopo il commit orfano,
`git rev-list --all --count` restituiva **2**, non 1: `refs/remotes/origin/master` (il
riferimento di tracking del remoto) puntava ancora alla storia vecchia e la rendeva
**raggiungibile**, quindi non prunabile. Cancellare solo il branch locale non basta: i
remote-tracking ref vanno rimossi esplicitamente, altrimenti la verifica "0 segreti nella
storia" è falsa. Lo stesso vale per `refs/remotes/origin/HEAD`, che era rimasto come
puntatore SHA1 nullo.

**Verifica definitiva (blob per blob, non solo working tree).**

```
$ git rev-list --all --count
1
$ git log --oneline --all
"<sha>" chore: pubblica PollingWeb su storia Git ripulita [T0.10]

# scansione di OGNI blob di OGNI commit raggiungibile
$ CNT=$(git rev-list --objects --all | awk '{print $1}' \
        | git cat-file --batch-check='%(objectname) %(objecttype)' \
        | awk '$2=="blob"{print $1}' | git cat-file --batch \
        | grep -acE "$PATTERNS")
$ echo "$CNT"
0                                  # su 164 blob scansionati
$ git fsck --unreachable           # nessun oggetto irraggiungibile
$ git count-objects -v             # count=0, garbage=0
$ du -sh .git
608K                               # era 17,03 MiB: i blob dei dump sono stati rimossi
```

`git grep` da solo **non** sarebbe bastato: guarda solo il working tree. La verifica è stata
fatta sull'object database completo, che è l'unica cosa che dimostra qualcosa su una storia.

**Redazione dei documenti di lavoro.** `PLAN.md` e `docs/LOOP_LOG.md` nominavano i vecchi
identificatori per descriverne la rimozione: sarebbero rimasti come riferimento all'azienda
nel repository pubblico. Sostituiti con placeholder e con un rimando al record di audit
(`REDACTED-VALUES.md` contiene la mappatura completa vecchio → nuovo). I pattern del record
di audit sono scesi a 16 voci, includendo ora anche i vecchi identificatori.

**Correzione di un'affermazione errata.** Fino a questo punto i documenti dicevano che la
repository era pubblica dal 2023. La verifica eseguita prima del push la smentisce:

```
$ gh repo view donatopepe/PollingWeb --json visibility,isFork,defaultBranchRef
{"defaultBranchRef":{"name":"master"},"isFork":false,"visibility":"PRIVATE"}
$ gh api repos/donatopepe/PollingWeb/collaborators --jq '.[].login'
donatopepe                      # unico collaboratore
$ gh api repos/donatopepe/PollingWeb/forks --jq 'length'
0
```

Quindi: **i segreti non sono stati esposti al pubblico**. Documenti e messaggio di commit
sono stati corretti. Sopravvalutare una violazione è un errore quanto sottovalutarla: la
prima porta a decisioni sbagliate (notifiche inutili, panico), la seconda a sottovalutare
il rischio.

**Cosa resta comunque da fare.**

1. **sostituire il contenuto remoto**: su GitHub c'è ancora `master` @ `6b01bfa` con i
   segreti. La riscrittura è solo locale finché non viene pubblicata (`docs/RELEASING.md` §2);
2. **ruotare** le credenziali **prima** della pubblicazione: non per un'esposizione
   avvenuta, ma perché la pubblicazione le esporrebbe, perché le password erano deboli di
   per sé (password = utente, `CHECK_POLICY = OFF`, `SID` replicabile) e perché non è
   verificabile che non ne esistano copie locali dal 2023;
3. abilitare *secret scanning* e *push protection* (richiede Pro o repo pubblico).

**Prossimo.** `T0.9` — nota di disclosure e rotazione credenziali; poi `T0.7` (schema unico) e
`T0.8` (deploy riproducibile), che sbloccano il track cross-platform.

**Blocchi.** Nessuno.

## 2026-10-16T12:00Z — T0.10 (seguito) — pubblicazione e limite degli oggetti orfani

**Stato remoto prima.** `gh repo view` → **`visibility: private`**, `isFork: false`,
`defaultBranchRef: master`, un solo collaboratore (il proprietario), 0 fork, nessuna
security analysis. `git ls-remote` → `HEAD` e `refs/heads/master` = `6b01bfa`.

Ho quindi **corretto la documentazione**: fino a quel momento affermava che la repository era
pubblica dal 2023. Era falso. I segreti non sono stati esposti al pubblico. Sopravvalutare una
violazione è un errore quanto sottovalutarla: la prima porta a decisioni sbagliate, la seconda
a ignorare il rischio. Le correzioni sono nel commit
`docs: correggi l'affermazione di esposizione pubblica dei segreti`, e i messaggi di commit
sono stati riscritti con `git filter-branch --msg-filter` per eliminare la stessa frase errata
dalla storia.

**Pubblicazione.**

```
$ git push -u origin main                    # nuovo ramo, nessun force necessario
$ gh repo edit donatopepe/PollingWeb --default-branch main
$ git push origin --delete master
 - [deleted]  master
$ git ls-remote origin
9e11839...  HEAD
9e11839...  refs/heads/main                  # unico ref: la storia con i segreti non è più puntata
```

**Limite accertato — il punto che conta.** Il commit con i segreti **resta recuperabile per
SHA**:

```
$ git fetch origin 6b01bfa9c39b92d156f0eaff5501498d37c5c56d
 * branch  6b01bfa9...  -> FETCH_HEAD        # riesce ancora, a ref rimossi
```

Non esistendo più ref che lo puntano, non è raggiungibile "navigando" la repository, ma chi ha
accesso e conosce lo SHA può ancora leggerlo. GitHub non compatta immediatamente gli oggetti
irraggiungibili. **Il push risolve il problema della pubblicazione imminente, non quello della
rimozione definitiva.** Documentato in `docs/SECURITY-FINDINGS.md` §5 e in
`docs/RELEASING.md` §2, con le tre strade: rotazione (garantita), richiesta a GitHub Support,
cancellazione e ricreazione della repository (qui accettabile: 0 fork, 0 issue, 1 collaboratore).

**Errore mio, durante la verifica.** Il `git fetch <sha>` appena descritto ha **scaricato gli
oggetti vecchi nel repository locale**, reintroducendo i segreti in `.git`. Me ne sono accorto
subito: ripulito con

```
$ rm -f .git/FETCH_HEAD
$ git reflog expire --expire=now --all && git gc --prune=now
$ git cat-file -e 6b01bfa9...   -> assente
```

e la procedura di pulizia è ora scritta in `docs/RELEASING.md`, perché è una trappola in cui
ricadrà chiunque faccia la stessa verifica.

**Verifica finale.**

```
commit raggiungibili: 3 (radice sanificata + 2 commit di registro)
blob con pattern del record di audit: 0 su 170
pattern nei messaggi di commit: 0
oggetti non raggiungibili: 0 (count=0, garbage=0)
.git: 17,03 MiB -> 616 KiB
refs locali: refs/heads/main -> 9e11839
refs remoti: HEAD, refs/heads/main -> 9e11839
```

**Prossimo.** `T0.9` — nota di disclosure in `SECURITY.md` con la procedura di rotazione;
poi `T0.7`/`T0.8`. Il loop cron viene riattivato.

**Blocchi.** Nessuno. Attenzione operativa: la rotazione delle credenziali è **ancora da
fare** ed è ora l'unica mitigazione garantita.

## 2026-10-16T12:15Z — T0.9 Nota di disclosure e rotazione credenziali — DONE

**Contesto.** `docs/SECURITY-FINDINGS.md` documenta i problemi, ma mancava il canale di
segnalazione, la policy di divulgazione e una dichiarazione di sicurezza di livello superiore.
La DoD richiede inoltre che le sezioni siano **linkate dal README** — che non esisteva.

**Modifiche.**

| File | Contenuto |
|---|---|
| `SECURITY.md` (nuovo) | 7 sezioni: versioni supportate · come segnalare (canale privato, cosa includere, tempi, fuori perimetro) · divulgazione coordinata (90 giorni) · **avviso storico** con la tabella di stato · **rotazione credenziali** · regole per chi contribuisce con i comandi di controllo · hardening per chi installa |
| `README.md` (nuovo, minimo) | descrizione, avviso di sicurezza in evidenza, architettura, struttura, documentazione, **stato della build verificato**, contribuzione, licenza |
| `LICENSE` (nuovo) | testo MIT, già promesso da `NOTICE` che lo referenziava senza che il file esistesse |

`docs/SECURITY-FINDINGS.md` §5 (procedura operativa di rotazione) esisteva già da `T0.1`/`T0.10`
ed è la destinazione dei link.

**Perché ho aggiunto README e LICENSE, che appartengono ad altri task.** La DoD di `T0.9`
pretende il link dal README: senza un README la verifica è impossibile, e la repository
pubblica non ne aveva alcuno. `NOTICE` referenziava già `LICENSE` senza che il file esistesse —
un riferimento rotto introdotto da me in `T0.4`. `D1` resta aperto per il README completo con
quickstart verificato su due OS; `D10` per audit delle licenze, header SPDX e
`docs/LICENSING.md`.

**Errore intercettato e corretto.** La prima stesura del README mostrava come quickstart:

```bash
dotnet build Database/Database.csproj
dotnet build PollingClass/PollingClass.csproj
```

Li ho **eseguiti** invece di assumerli. Falliscono entrambi (`MSB3644`: mancano i targeting
pack `v4.5`), e `dotnet build PollingWeb.sln` fallisce con `MSB4249` (il compilatore ASP.NET
esiste solo in MSBuild di .NET Framework, su Windows). Il README è stato riscritto con una
sezione **"Stato della build (verificato, non ipotetico)"** che riporta i tre errori reali con
il testo esatto e la tabella causa → task che li risolve. Un README che promette un comando
che non funziona è peggio di un README che dichiara di non essere ancora buildabile.

**Fatti nuovi registrati nel piano** (`PLAN.md` §2.2):

- **P13** — `MSB4249`: il sito WebForms **non è compilabile in CI su Linux**. Giustifica `X1.7`
  (esclusione dal build di default) e `X4.2` (porting a Razor Pages).
- **P14** — `MSB3644`: i `.csproj` classici dichiarano `v4.5` senza targeting pack, quindi
  `dotnet build <progetto>.csproj` fallisce su **qualsiasi** OS. Giustifica `X1.1`–`X1.3`.

**Evidenza.**

```
$ for f in README.md SECURITY.md; do ... verifica di ogni link relativo ... done
README.md   -> docs/LOOP_LOG.md docs/RELEASING.md docs/SECURITY-FINDINGS.md
               LICENSE NOTICE PLAN.md samples/README.md SECURITY.md sql/README.md   (9 link, tutti OK)
SECURITY.md -> docs/RELEASING.md docs/SECURITY-FINDINGS.md                          (2 link, OK)
$ grep -c '^## ' SECURITY.md              -> 7
$ grep -n '^## 5\.' docs/SECURITY-FINDINGS.md
217:## 5. Piano di rotazione credenziali (operativo — a carico dell'ente)
$ grep -c 'SECURITY.md' README.md          -> 5 occorrenze (link in evidenza)
$ git grep -inE "$PATTERNS" -- README.md SECURITY.md LICENSE NOTICE   -> exit 1 (0 risultati)
$ dotnet build Database/Database.csproj      -> MSB3644 (documentato, non nascosto)
$ dotnet build PollingClass/PollingClass.csproj -> MSB3644 (idem)
$ dotnet build PollingWeb.sln                -> 2 errori MSB4249 (idem)
```

**Punto aperto per D10.** Il titolare del copyright in `LICENSE` è indicato come
"PollingWeb contributors": è una convenzione sicura, ma il proprietario della repository deve
confermarlo o sostituirlo con il proprio nome prima della `v1.0.0`.

**Prossimo.** `T0.7` — consolidamento dello schema SQL in `sql/` come fonte unica di verità,
con ADR. Serve prima di `T0.8` (`scripts/db-deploy.sh`) e di tutto il track cross-platform.

**Blocchi.** Nessuno.
