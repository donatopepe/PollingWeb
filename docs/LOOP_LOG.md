# LOOP LOG — PollingWeb remediation

## 2026-10-17T17:17Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** T3 lists `Datini` and `Datfin`, but no integration test directly invokes them.

**Esito atteso:** SQL boundary functions return exact expected values for a synthetic leap-day input: `Datini` at end-of-day and `Datfin` at midnight, independent of language date formatting.

**Modifica:** added a database integration test using ISO 8601 conversion and compares exact `DateTime` values for both functions.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori (warning preesistenti); focused SQL test `Datini_and_Datfin_return_expected_synthetic_boundaries` → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con ulteriori coperture mancanti secondo DoD.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.


## 2026-10-17T17:07Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** `Flusso_Ordini_SAP_ist_executes_with_synthetic_nb_item_and_rollback` exercises the selected institute's order but did not add a second ready order from a different institute to ensure the filter excludes it.

**Esito atteso:** the focused transaction executes the target-institute order and leaves the foreign-institute ready order out of the output for this session.

**Modifica:** attempted to assert a master output row by parsing its legacy fixed-width format. Baseline failure showed that assumption was invalid (`Synthetic ODA institute flow produced no master row for requested institute`), so both new assertions were removed. No valid improvement remained within this attempt; the baseline test was restored unchanged. Existing output-scope coverage from iteration 72 remains intact.

**Evidenza reale:** after restoring the baseline test, `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori; focused SQL test `Flusso_Ordini_SAP_ist_executes_with_synthetic_nb_item_and_rollback` → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con ulteriori coperture mancanti secondo DoD.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.


## 2026-10-17T17:00Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** `Flusso_Ordini_SAP_ist_executes_with_synthetic_request_inside_rollback` checked only that the current session received some ODA output, not that the emitted rows were all scoped to the requested institute.

**Esito atteso:** the synthetic institute flow emits a master ODA record for institute 1 and emits no ODA rows with another or missing institute code.

**Modifica:** tightened the existing rollbackable flow assertion to require a master record with `Codist=1`, plus an assertion rejecting any same-session ODA output outside that institute.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori; focused SQL test `Flusso_Ordini_SAP_executes_with_synthetic_request_inside_rollback` → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con ulteriori coperture mancanti secondo DoD.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.


## 2026-10-17T16:50Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** `ImportaBudget_inserts_then_updates_synthetic_budget_and_rolls_back` checks update isolation from other institutes, but does not check that updating one synthetic budget leaves unrelated budgets in the same institute active.

**Esito atteso:** after the update call, only the previous version for the same SAP ID and material is deactivated; unrelated existing synthetic budget rows remain active.

**Modifica:** added an assertion on the 2098 synthetic budget row with a different SAP ID/material, requiring it not to be marked inactive.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori; focused SQL test `ImportaBudget_inserts_then_updates_synthetic_budget_and_rolls_back` → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con ulteriori coperture mancanti secondo DoD.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.


## 2026-10-18T00:00Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** test `AggiornaFornitori_updates_multiple_synthetic_suppliers_for_one_institute` already checks two synthetic suppliers are updated for institute 32003, but not that the resulting update stayed within that institute.

**Esito atteso:** all matching supplier updates are confined to institute 32003; all other institutes remain unaffected.

**Modifica:** added rollbackable `IF EXISTS` assertion rejecting any updated supplier row for a different non-null institute.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori (warning preesistenti); focused SQL test → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con ulteriori coperture mancanti secondo DoD.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.


Registro append-only delle iterazioni del loop.

## 2026-10-17T09:04Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** `ImportaBudget_inserts_then_updates_synthetic_budget_and_rolls_back` verifica inserimento, update e disattivazione del budget precedente per il solo istituto sintetico; non controlla possibili effetti su altri istituti.

**Esito atteso:** dopo le due chiamate alla stored procedure, nessun budget attivo appartenente a un altro istituto deve essere disattivato.

**Modifica:** aggiunta asserzione nella stessa transazione/rollback, dopo l'update, che non esistano righe `IsActive=1` di istituti diversi da quello sintetico.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori. Test mirato con configurazione SQL del container caricata senza stamparla → `ImportaBudget_inserts_then_updates_synthetic_budget_and_rolls_back` 1/1 pass, exit 0. `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con un altro oggetto o ramo non ancora coperto.

**Blocchi:** nessuno per questa verifica; T3 resta incompleta.

---

## 2026-10-17T08:49Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Esito atteso:** il test errore sintetico passa e assicura che la RdA in stato 16 non generi anche un log di transizione successo (stato 20).

**Baseline riprodotto:** il test mirato pre-modifica `Esiti_Ordini_SAP_executes_with_synthetic_error_result_and_rollback` → 1/1 pass con SQL Server.

**Modifica:** aggiunta asserzione rollbackabile `IF EXISTS` sul GUID per escludere qualsiasi riga `LogTime_Ciclo` con `StatoID=20`/`StatoIntID=20`.

**Verifica reale:** build IntegrationTests → exit 0, 0 errori; test SQL mirato post-modifica → 1/1 pass, exit 0; `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con un altro ramo/oggetto richiesto dalla DoD.

**Blocchi:** nessuno per questo caso; T3 resta incompleta.

---

## 2026-10-17T08:03Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** il caso errore di `Esiti_Ordini_SAP` copre archiviazione, status RdA, log unico e output vuoto. Un controllo esplorativo che si aspettava la rimozione della riga sorgente LastOutput_OnLine è fallito (`SAP error output row was not consumed`); questa semantica non viene assunta.

**Modifica:** aggiunte verifiche che il log errore riporti `Matricola=N'BBSISTE'` e preservi il `Rch_Progr`/`PageID` della richiesta sintetica, come selezionati dalla procedura. Fixture in transazione rollbackabile. Nessuna modifica alla stored procedure.

**Evidenza reale:** build IntegrationTests → exit 0, 0 errori. Test SQL mirato `Esiti_Ordini_SAP_executes_with_synthetic_error_result_and_rollback`, con configurazione container caricata senza stamparla → 1/1 pass, exit 0. `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con un altro ramo/oggetto della DoD.

**Blocchi:** nessuno per questo caso; T3 resta incompleta.

---


**Baseline:** `Esiti_Ordini_SAP_executes_with_synthetic_error_result_and_rollback` fissa archiviazione, stato RdA, log e output vuoto per un errore sintetico. Un controllo esplorativo che si aspettava la rimozione della riga sorgente LastOutput_OnLine è fallito (`SAP error output row was not consumed`); il controllo non era giustificato dal comportamento osservato della procedura ed è stato rimosso.

**Modifica:** nessuna modifica applicativa mantenuta: il test è stato riportato alla verifica delle semantiche effettivamente confermate. Registrato l'esito negativo dell'ipotesi di consumo/cancellazione della riga sorgente, senza alterare la stored procedure.

**Evidenza reale:** build IntegrationTests → exit 0, 0 errori. Test mirato con configurazione SQL del container caricata senza stamparla → dopo la rimozione dell'asserzione esplorativa, 1/1 pass, exit 0. `git diff --check` → exit 0.

**Prossimo task:** continuare T3 selezionando un ramo/oggetto richiesto dalla DoD con semantica verificabile.

**Blocchi:** nessuno; nessun nuovo comportamento assunto.

---

## 2026-10-17T07:34Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** `Esiti_Ordini_SAP_executes_with_synthetic_error_result_and_rollback` controllava l'archiviazione dell'errore e `RdA.Status=16`, ma non fissava che il log della transizione di errore venga scritto una volta sola. Inizialmente è stata provata anche un'asserzione su `RdE.Status=16`; il test ha fallito con `Synthetic response status was not updated for SAP error`. Ispezionando la stored procedure si è verificato che il ramo errore aggiorna RdA e il log, non RdE; l'asserzione non supportata è stata rimossa.

**Modifica:** il test ora richiede esattamente un record `LogTime_Ciclo` per il GUID sintetico con `StatoID=16` e `StatoIntID=16`, e controlla che `@po_valoreuscita` sia vuoto. Le fixture sono dentro la transazione rollbackabile.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → compilazione completata, 0 errori, exit 0. Il test SQL mirato, con configurazione container caricata senza stamparla, → 1/1 pass, exit 0. `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con un altro ramo/oggetto richiesto dalla DoD.

**Blocchi:** nessuno per questo test; T3 resta incompleta.

---

## 2026-10-17T07:02Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** il caso `Esiti_Ordini_SAP_converts_synthetic_warning_to_success_and_archives_original` verificava gli stati finali RdA/RdE ma non controllava l'esatto numero di righe di log per la transizione di successo.

**Modifica:** aggiunta asserzione che il GUID sintetico abbia esattamente un log con `StatoID=20` e `StatoIntID=20`; resta nella transazione con rollback del test.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0, 0 errori. Test mirato eseguito con configurazione SQL del container caricata senza stamparla: `dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-build --no-restore --nologo -v quiet --filter FullyQualifiedName~Esiti_Ordini_SAP_converts_synthetic_warning_to_success_and_archives_original` → 1/1 pass, exit 0. `git diff --check` → exit 0. Build soluzione non parte dal task legacy già noto; nell'iterazione precedente `dotnet build PollingWeb.sln --nologo -v quiet` ha prodotto MSB4249.

**Prossimo task:** continuare T3 con un altro ramo/oggetto richiesto dalla DoD.

**Blocchi:** nessuno per questo caso; T3 resta incompleta.

---

## 2026-10-17T06:36Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline:** il test `AzzeraTargetSC00G100_deletes_only_requested_synthetic_institute` cancellava e verificava una sola riga. Non dimostrava che l'operazione fosse limitata all'istituto richiesto.

**Modifica:** aggiunta una seconda riga sintetica per un altro istituto nella stessa transazione; il test ora verifica sia la cancellazione del target selezionato sia la conservazione dell'altro. Entrambe le righe sono rollbackate.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → compilazione completata, 0 errori, exit 0. Prima esecuzione mirata senza `POLLING_DB_PASSWORD` → 1 ignorato; poi, caricando la configurazione di test dal container senza stamparla, `dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-build --no-restore --nologo -v quiet --filter FullyQualifiedName~AzzeraTargetSC00G100` → 1/1 pass, exit 0. `git diff --check` → exit 0.

**Prossimo task:** continuare T3 con un altro ramo/oggetto mancante.

**Blocchi:** nessuno per questo caso; T3 rimane incompleta.

---

Formato riga:
`## <timestamp UTC> — <TASK ID> <titolo> — <stato>`
poi: contesto, modifiche, evidenza (comando + output), prossimo task, blocchi.

---

## 2026-10-17T12:00Z — C8 Percorsi file configurabili — COMPLETATO

**Baseline:** `rg` ha trovato path costruiti tramite separatore Windows fisso in `ExportSQL01`, `TaskServiceClass`, `ImportFatture`, `ParserFatture`, `LeggiFornitore`, `Ordini_Sap` e `Import_Budget`. La scansione dei path assoluti `C:\\`/`D:\\` nel codice non ha trovato corrispondenze.

**Modifica:** aggiunto `ConfiguredFilePaths.Combine`, che richiede directory configurata esistente e solo un nome file (senza path), usando `Path.Combine`. Tutti gli accessi attivi ai file dei flussi citati usano l'helper; rimossi due commenti obsoleti con concatenazione Windows. Aggiunti test per output cross-platform, directory mancante e nome file con componenti path.

**Evidenza reale:** `dotnet build PollingClass/PollingClass.csproj -f net8.0 --nologo -v quiet` → exit 0; stesso comando con `-f net48` → exit 0. `dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet` → superato, 115 passati/2 ignorati. Scansione dei path assoluti Windows in `Database`/`PollingClass` → 0 risultati; scansione dei pattern concatenazione `path_source + @"\\"`/`PATH_FOLDER + @"\\"`/`archivia_path + @"\\"` → 0 risultati. `git diff --check` → exit 0. La soluzione completa non è stata verificata in questa iterazione.

**Prossimo task:** `C9 — Robustezza parsing tracciati SAP`.

---

## 2026-10-17T12:20Z — C9 Robustezza parsing SAP — IN PROGRESS


**Baseline riprodotto:** `Ordini_Sap` esegue `Substring` su righe senza controllare la lunghezza minima; `ImportaXML` assume foglio `BDG`, dati worksheet e riferimenti alle shared strings validi; `ParserFatture` usa `StreamReader` predefinito e `IsNumeric` dipende dalla cultura. Evidenza attesa: gestione esplicita input vuoti/mancanti/troncati e encoding dichiarato, fixture sintetiche, build PollingClass su entrambi i TFM e test unit.

**Modifiche parziali:** `Ordini_Sap` ignora righe vuote e rifiuta record troncati; `ImportaXML` valida foglio/dati e riferimenti shared-string, restituisce empty per cella assente e tollera righe budget con celle mancanti; `ConfiguredFilePaths` dichiara UTF-8/BOM e decode Windows-1252 esplicito; `ParserFatture` ignora input vuoto, rifiuta header corto, usa invariant parsing e formati data espliciti. `samples/sap/README.md` documenta i formati e i campioni sintetici. Test generati coprono output troncato, righe vuote, UTF-8/BOM, Windows-1252, date sotto culture diverse e workbook incompleti.

**Verifica reale (ultima esecuzione, Linux):** `dotnet build PollingClass/PollingClass.csproj -f net8.0 --nologo -v quiet` → exit 0; stesso comando con `-f net48` → exit 0; `dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet` → 140 passati/2 ignorati; `git diff --check` → exit 0. Il comando `dotnet build PollingWeb.sln --nologo -v quiet` era stato eseguito in precedenza in questa iterazione: exit 1, MSB4249 WebForms.

**Esito:** dopo tre iterazioni sul task, C9 è marcato `[!]` e bloccato. Restano mancanti test end-to-end di `ParserFatture` e `Flusso_*`: crearli richiede fixture SAP complete approvate e schema/DDL verificati (in particolare le tabelle mancanti registrate in T0.13). Mitigazione: mantenuti i test deterministici sui boundary verificabili; riprendere l'end-to-end solo dopo acquisizione della DDL e fixture sintetiche conformi, senza dedurre il contratto dai soli parser.

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

## 2026-10-16T12:45Z — T0.7 Schema SQL: fonte unica di verità — DONE

**Contesto.** Due database project SSDT con contenuto dichiaratamente identico, uno dei quali
non compilava nulla, più uno snapshot Quartz in UTF-16. Nessuna fonte di verità dichiarata.

**Confronto hash sui 26 file prima di toccare qualsiasi cosa:**

```
comuni identici: 25
comuni DIVERSI:   1   -> Security/01-create-app-login.sql
                       (DatabaseSQL: 1177 byte  vs  PollingDB: 0 byte)
```

**Bug trovato: un file di sicurezza vuoto che nessuno segnalava.** `PollingDB` — il progetto
che compila davvero — aveva `Security/01-create-app-login.sql` **vuoto**. La causa era un `sed`
di `T0.2` che leggeva e riscriveva lo stesso file: la shell lo troncava prima della lettura.
Effetto reale: il `<Build Include>` puntava a uno script vuoto, quindi un deploy basato su quel
progetto **non avrebbe mai creato il login applicativo**, in silenzio. Ripristinato dalla copia
intatta invece di "rigenerarlo", così il contenuto non è congetturale.

Questo è il motivo per cui il confronto hash è stato fatto **prima** di cancellare: i due
alberi non erano duplicati puri finché quel file non è stato riparato.

**Contesto che ha cambiato la decisione.** `DatabaseSQL.sqlproj` ha **0 `<Build Include>`**:
non compila nulla, i suoi 26 file sono orfani. Il percorso di deploy non poteva quindi restare
SSDT: si compila solo su Windows (`MSB4249`, fatto `P13`) e il progetto punta a girare su Linux.

**Decisione (ADR 0001).** `sql/` è la fonte unica di verità; gli script sono autorevoli, il
`.sqlproj` resta come comodità opzionale per Visual Studio; `DatabaseSQL/` è eliminato.

Modifiche: `PollingDB/` → `sql/PollingDB/`; `sql/quartz-schema.sql` → `sql/Quartz/`;
`DatabaseSQL/` rimosso (26 file, duplicati puri); nuovi `sql/eProcurement/README.md`,
`sql/deploy/README.md`, `sql/README.md` riscritto, `docs/adr/0001-*.md`;
`docker-compose.yml` + `scripts/db-deploy.sh`.

**Ostacoli di piattaforma risolti durante la verifica.**

1. **Docker e spazio su disco.** `/` era al 96% e il pull dell'immagine SQL Server (~2,9 GB)
   l'ha portato al 98%, 2,3 GB liberi. I dati del container sono stati spostati **fuori** dal
   filesystem di sistema.
2. **Il bind-mount non funzionava su `/mnt/ssd-transfer`**: è `exfat`, che non supporta
   `chown`; SQL Server gira come uid 10001 e non poteva scrivere
   (`Directory [/var/opt/mssql/.system/system] could not be created`). Spostato su
   `/home/public` (`ext4`, 23 GB liberi) → container `healthy`.
3. **BOM vs `sqlcmd` su stdin.** Tutti i `.sql` hanno il BOM UTF-8: con `sqlcmd -i` è gestito,
   ma su **stdin** finisce nella prima istruzione →
   `Msg 102: Incorrect syntax near ''`. Aggiunta la funzione `emit_sql` che lo elimina al volo
   senza toccare i file.
4. **Lo script Quartz falliva su database vuoto**: iniziava con 4
   `ALTER TABLE ... DROP CONSTRAINT` non condizionati su tabelle inesistenti. Resi condizionali
   con `IF EXISTS` su `sys.foreign_keys`.
5. **Bug nel seed sintetico, trovato solo eseguendolo**:
   `String or binary data would be truncated in column 'Codfis'` — `NVARCHAR(16)` e io avevo
   scritto 17 caratteri. Corretto in `samples/seed/seed.sql` **e** in
   `samples/sap/SC00G100.sample.txt`, per coerenza. È esattamente il tipo di difetto che un
   sample mai eseguito nasconde.

**Scoperta di portata maggiore: lo schema `eProcurement` non è versionato.** Analisi dei
riferimenti espliciti `eProcurement.dbo.*`: **19 oggetti, di cui 16 assenti dal repository**.
Il deploy quindi **riesce** (name resolution differita) ma produce un database vuoto: le
stored procedure falliscono alla prima esecuzione. Il sistema non è distribuibile nella sua
interezza. Registrato come **T0.12** (recupero della sola DDL dallo snapshot nel record di
audit offline) e documentato in `sql/eProcurement/README.md` e ADR 0001 §4.

**Evidenza.**

```
$ POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed     # esecuzione 1
  database: Polling, eProcurement, Quartz (ricreati)
  schema applicativo: 23 file applicati
  Quartz: quartz-schema.sql
  login: saltato (POLLING_APP_PASSWORD non impostata: nessuna credenziale inventata)
  seed: samples/seed/seed.sql
  EXIT=0
$ POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --with-seed     # esecuzione 2 (idempotenza)
  EXIT=0

Polling : tabelle=11  viste=1  funzioni=3  procedure=8
Quartz  : tabelle=11 (QRTZ_*)
eProcurement: tabelle=0                      <- conferma il gap T0.12
dati sintetici: DimStato=6 DimProcessi=5 TaskService=3 TargetSC00G100=5
ViewTaskService -> 3 righe                    <- la vista unisce TaskService e DimProcessi
dbo.Datini('2024-03-15') -> 2024-03-15 23:59:59.997
dbo.Datfin('2024-03-15') -> 2024-03-15 00:00:00.000
```

**Nota su un test che non testava nulla.** Il primo smoke test delle due funzioni usava
`CONVERT(varchar(10), ...)`, che tronca la componente oraria: `Datini` e `Datfin` restituivano
la stessa stringa e il test non distingueva i due rami. Rifatto con `varchar(23)`: si vede che
`Datini` è la *fine* giornata e `Datfin` l'*inizio*. I nomi sono fuorvianti, ma il
comportamento resta invariato (rinominarli romperebbe il contratto con le procedure).

**Prossimo.** `T0.12` — recupero dello schema `eProcurement` (solo DDL) dallo snapshot nel
record di audit offline.

**Blocchi.** Nessuno. Il container `pollingweb-mssql` resta attivo per le iterazioni
successive; i dati sono in `/home/public/.pollingweb-mssql` (fuori dal repository, ext4).

## 2026-10-16T13:20Z — T0.12 Recupero dello schema eProcurement (solo DDL) — DONE

**Contesto.** `T0.7` ha reso `sql/` la fonte unica di verità, ma ha anche reso visibile che il
database `eProcurement` non era versionato: 19 oggetti referenziati dal codice, 16 assenti. Il
deploy riusciva (SQL Server non valida gli oggetti referenziati alla creazione) e produceva un
database vuoto. Il sistema non era distribuibile nella sua interezza.

**Fonte.** Lo schema esisteva solo nello snapshot `script_eprocurement_donato.sql`, rimosso dal
repository in `T0.3` perché conteneva 15.732 `INSERT` di dati reali. Recuperato **dal record di
audit offline** (`full-history.bundle`), clonato in `/tmp/legacy` — **fuori dal repository**,
come da procedura in `docs/RELEASING.md` §1.

**Metodo: scartare i dati *prima* di scrivere.** L'estrattore divide lo script in batch
delimitati da `GO`, li classifica e scarta i dati **prima** di comporre qualunque file. Non
esiste un percorso in cui un `INSERT` venga scritto e poi rimosso: i batch `INSERT` non
arrivano mai alla fase di scrittura.

```
batch scartati: INSERT=15728  SET IDENTITY_INSERT=4  extprop=8  USE=1
DROP CONSTRAINT attribuiti alle tabelle: 27
batch mantenuti: 100   batch orfani: 0
oggetti scritti: 17  (= 15 tabelle, 1 vista, 1 funzione)
sorgente 12.435 KB  ->  risultato 25 KB
```

**Tre errori miei, trovati verificando e non assumendo.**

1. **`SET IDENTITY_INSERT` finiva dentro il file della vista.** Quei batch accompagnano i
   blocchi di dati e non sono `INSERT`; non essendo filtrati, venivano attribuiti all'oggetto
   corrente. La scansione `grep -ric INSERT out/` ha restituito 4 occorrenze *dentro*
   `ViewEstrazioneRdARdE.sql`: senza quella verifica avrei committato righe di dati in un file
   di schema. Aggiunto `IDENTITY_RE`.
2. **27 `DROP CONSTRAINT` finivano fra gli "orfani"** e venivano persi, perché precedono
   qualunque commento di oggetto. Attribuiti alla tabella di destinazione (parsando il nome
   dall'istruzione), non scartati: servono a rendere lo schema riapplicabile.
3. **`DROP VIEW` e `DROP FUNCTION` non erano condizionati**, quindi fallivano su database
   nuovo — lo stesso difetto già corretto nello script Quartz in `T0.7`. Il primo tentativo di
   protezione non ha funzionato perché il batch *inizia con un commento*, quindi `^` ancorato
   all'inizio del batch non lo intercettava. Corretto esaminando ogni riga e preservando il
   commento.

Bonus della stessa natura: la prima protezione generava
`OBJECT_ID(N'...', 'FN', N'IF', N'TF')` — `OBJECT_ID` accetta **un solo** argomento di tipo.
Sostituita con la forma senza tipo.

**Un `grep` che mi ha quasi fatto sbagliare.** Avevo concluso che i vincoli fossero assenti
dallo snapshot: `grep -c 'ADD CONSTRAINT'` dava 0. Il dump usa `ADD  CONSTRAINT` con **due
spazi**. Rifatta la verifica con whitespace flessibile: 27 `ADD CONSTRAINT`, 5 `FOREIGN KEY`,
22 `DEFAULT`, 13 `PRIMARY KEY` inline. Il pattern matching con spaziatura fissa su SQL
generato è una trappola: sarebbe stato un falso allarme costoso (avrei "ricostruito" vincoli
già presenti).

**A2 risolta con due prove convergenti.** `RDA_Esiti_LastOutput_OnLine` e
`Tbl_Export_SAP_OnLine` appartengono a `eProcurement`:

1. le stored procedure le referenziano **sempre** qualificate `eProcurement.dbo.*`;
2. i TableAdapter che le usano ricevono `new DatabaseInfo(true)`, e il parametro `true`
   seleziona `StringaConnessioneEproc`.

Spostate in `sql/eProcurement/dbo/Tables/`, rimosse da `sql/PollingDB/` e dal suo `.sqlproj`.

**Fedeltà verificata, non dichiarata.**

```
                                    snapshot   dopo il deploy
tabelle                                    15   15
viste                                       1    1
funzioni                                    1    1
chiavi primarie                            13   13
chiavi esterne                              5    5
vincoli DEFAULT                            22   22
indici non-cluster                          0    0        (quindi A5 chiusa: non mancanti)

./scripts/db-deploy.sh --with-seed   ->  EXIT=0
./scripts/db-deploy.sh --with-seed   ->  EXIT=0   (idempotente)
dbo.StripHTML('<p>Ciao <b>mondo</b></p>')  ->  ' Ciao  mondo'
SELECT COUNT(*) FROM ViewEstrazioneRdARdE  ->  0
```

**Controlli anti-dati sul contenuto committato.**

```
pattern del record di audit (host, login, password, marchi)  -> 0
INSERT / SET IDENTITY_INSERT                                 -> 0
email, partite IVA                                           -> 0
DEFAULT presenti                                             -> (0), (1), getdate(), ('Y')
```

**Nuovi problemi aperti, emersi dal lavoro.**

- **A6 → T0.13**: 4 tabelle usate da `ParserFatture.cs` (`Tab_SC00S100_1Ord`, `_1Ord_R`,
  `_2Fatt`, `_2Fatt_R`) non esistono in nessuno schema versionato **né nello snapshot**. La
  DDL non è disponibile da nessuna fonte: serve l'ambiente di origine. Task marcato come
  dipendente dall'esterno: **non** va chiuso ricostruendo colonne a intuito.
- **A7 → C11**: `Esiti_Ordini_SAP.sql` esegue `INSERT INTO RDA_Esiti` non qualificata, ma la
  procedura gira sulla connessione applicativa e la tabella sta in `eProcurement`. A runtime:
  `Invalid object name 'RDA_Esiti'`. Difetto preesistente, non nascosto dal deploy.

**Pulizia.** `/tmp/legacy` conteneva lo snapshot completo con i 15.732 `INSERT` di dati reali e
il clone della storia pre-riscrittura: rimosso a fine iterazione.

**Prossimo.** `T0.8` — `scripts/db-deploy.sh` a livello di produzione: utente a privilegi
minimi, verifica di idempotenza in `apply`, e chiusura dei punti A3 (migrazioni).

**Blocchi.** Nessuno.

## 2026-10-16T13:45Z — T0.13 Tabelle `Tab_SC00S100_*` — BLOCCATO `[!]`

**Esito: task marcato `[!]`, non chiuso.** La DoD stessa lo prescriveva: «se l'ambiente di
origine non è disponibile, il task va marcato `[!]` con il motivo, non chiuso a intuito».
L'ho fatto, dopo aver verificato che il motivo regge.

**Ricerca esaustiva, non un tentativo.** Prima di dichiarare il blocco ho interrogato ogni
fonte plausibile:

| Fonte | Esito |
|---|---|
| `sql/PollingDB/`, `sql/eProcurement/` | assenti |
| snapshot `eProcurement` (i 17 oggetti di `T0.12`, 15 tabelle) | assenti |
| 9 commit della storia archiviata, **tutti i blob** (non solo i path) | i nomi compaiono in **8 blob, tutti versioni di `ParserFatture.cs` e `Check_RdA.cs`**. Nessuno script, nessuna DDL |

```
$ git rev-list --objects --all | ... | while read b; do git cat-file blob "$b" | grep -qaE 'Tab_SC00S100' && echo $b; done
  8 blob -> solo codice applicativo
```

Il blocco è quindi motivato da un'assenza verificata, non da una difficoltà.

**Deliverable prodotto comunque.** Il task non è chiudibile, ma non è nemmeno inutile: ho
estratto dal codice la specifica delle colonne effettivamente usate, per tabella e per
operazione, in [`sql/eProcurement/MISSING-TABLES.md`](../sql/eProcurement/MISSING-TABLES.md).
Contiene anche le righe di riferimento e il testo della richiesta da inoltrare al cliente.

```
Tab_SC00S100_1Ord    INSERT 10 colonne (Tab_Name, Tab_Prog, Istituto, Data_Rif, NOrdine, Tipo, CodFor, RagSoc, GrA, Data_OrdAcq) ; SELECT (filtro NOrdine)
Tab_SC00S100_1Ord_R  INSERT  8 colonne ; SELECT (NOrdine, Perc_DaFatt) ; UPDATE 5 colonne (le *_DaFatt, mai inserite -> devono essere nullable)
Tab_SC00S100_2Fatt   INSERT  3 colonne ; SELECT (NOrdine, NFatt)
Tab_SC00S100_2Fatt_R INSERT  6 colonne ; SELECT (NOrdine, NFatt, NPos)
```

**Due errori dell'estrattore, trovati perché ho confrontato l'elenco con l'SQL originale.**

1. **Tutte le `SELECT` sparivano.** Il nome della tabella compare nel frammento `FROM`, non nel
   `SELECT`, e la mia ricerca della parola chiave avveniva solo sulla riga della tabella.
2. **Le colonne di una tabella finivano in un'altra.** La finestra di righe catturava
   l'istruzione successiva: `Tab_SC00S100_1Ord_R` compariva fra le colonne di
   `Tab_SC00S100_1Ord`. Causa più sottile di quanto sembri: `Tab_SC00S100_1Ord` è **prefisso**
   di `Tab_SC00S100_1Ord_R`, quindi l'attribuzione per elenco assegnava le istruzioni della
   tabella `_R` a quella senza suffisso. Risolto attribuendo **dal nome più lungo al più corto**.

Senza il confronto con l'SQL grezzo avrei pubblicato una specifica sbagliata, e la specifica è
l'unica cosa che questo task consegna.

**Impatto dichiarato, non nascosto.** `ParserFatture` — l'importazione fatture — non può
funzionare: tutte e quattro le tabelle sono coinvolte in ogni esecuzione. Il criterio **AC3**
non è raggiungibile per quel processo finché la DDL non arriva. Il resto della pipeline non è
bloccato: il deploy crea 15 tabelle in `eProcurement` e funziona.

**Perché non ho ricostruito le tabelle.** Il codice converte i valori con `getdouble`,
`getInt`, `TextToDate`: tipi indovinati produrrebbero errori di conversione o troncamenti
silenziosi sui dati reali. E le colonne elencate sono quelle **usate dal codice**, non tutte
quelle che la tabella reale possiede — le procedure `Flusso_*` possono usarne altre. Una
specifica congetturale sarebbe peggio di un blocco dichiarato: darebbe l'illusione di un
repository distribuibile.

**Sblocco.** Serve dall'ambiente di origine, in ordine di preferenza: script `CREATE TABLE`
completo di indici e vincoli; oppure l'output di `INFORMATION_SCHEMA.COLUMNS`; oppure un export
di schema completo del database `eProcurement`, da cui estrarre la sola DDL con la procedura di
`T0.12` (che scarta i dati **prima** di scrivere).

**Modifiche.** `sql/eProcurement/MISSING-TABLES.md` (nuovo), `PLAN.md` (T0.13 `[!]` + registro +
nota in §4: il blocco non ferma la pipeline), `docs/adr/0001` (A6), `sql/README.md`,
`README.md`.

**Prossimo.** `T0.8` — deploy a livello di produzione: utente a privilegi minimi, verifica di
idempotenza in `apply`, chiusura di A3 (migrazioni incrementali).

**Blocchi.** `T0.13` in attesa dell'utente. Nessun altro.

## 2026-10-16T14:20Z — T0.8 Deploy con utente a privilegi minimi — DONE

**Contesto.** Lo script esisteva e funzionava, ma il passo di sicurezza consegnava **solo
`GRANT CONNECT`**: la DoD chiede «utente applicativo a privilegi minimi» e non era consegnata.
Riprodotto eseguendo lo script: il passo saltava del tutto senza `POLLING_APP_PASSWORD`, e con
essa concedeva solo la connessione.

**Modifiche.**

| File | Intervento |
|---|---|
| `sql/PollingDB/Security/Permissions.sql` | riscritto: permessi oggetto per oggetto (9 tabelle, 1 vista, 3 funzioni, 8 procedure) |
| `sql/eProcurement/Security/Permissions.sql` | **nuovo**: 15 tabelle, 1 vista, 1 funzione |
| `sql/Quartz/Security/Permissions.sql` | **nuovo**: 11 tabelle |
| `scripts/db-deploy.sh` | esecutore unico con credenziali parametriche; permessi per database; **autotest** (passo 6/7); il deploy esce 1 se un controllo fallisce |
| `sql/deploy/README.md`, `README.md` | modello dei permessi, evidenza, procedura di rigenerazione |

I tre file sono **generati** dallo schema deployato, non scritti a mano: 38 oggetti trascritti
manualmente sono 38 occasioni di errore, e l'errore si manifesterebbe a runtime come "permesso
negato".

**Autotest: 8 controlli positivi e 4 negativi.** I negativi sono la parte che conta: verificano
che l'utente applicativo **non** possa fare ciò che non deve. Un test negativo che passa è un
difetto, non un successo, e il deploy fallisce.

```
ok  lettura tabella applicativa          ok  DDL negata (CREATE TABLE)
ok  scrittura tabella applicativa        ok  DDL negata su eProcurement
ok  lettura vista                        ok  DDL negata su Quartz
ok  invocazione funzione scalare         ok  scrittura su database di sistema
ok  esecuzione stored procedure
ok  scrittura su eProcurement            (l'insert cross-database funziona:
ok  lettura anagrafica fornitori          le procedure scrivono su eProcurement)
ok  accesso al job store Quartz
Autotest dei permessi: superato.
```

**Tre errori miei, tutti trovati eseguendo i test invece di assumerli.**

1. **`GRANT SELECT` su una funzione scalare è rifiutato**: `Msg 4606: Granted or revoked
   privilege SELECT is not compatible with object`. Il permesso corretto è `EXECUTE`. Non me ne
   sono fidato della memoria: ho concesso `EXECUTE` e poi invocato la funzione come utente
   applicativo.
2. **Sostituzione troppo larga nel generatore.** Per correggere il punto 1 ho sostituito la
   riga `GRANT SELECT ...` con `GRANT EXECUTE ...`: la stessa espressione era usata anche per
   le **viste**, che sono diventate `GRANT EXECUTE ON [dbo].[ViewTaskService]` → nuovo `4606`.
   Le viste richiedono `SELECT`, le funzioni `EXECUTE`: associazione inversa, stessa classe di
   errore. Corretto in modo esplicito per sezione, non con una sostituzione globale.
3. **Harness di test rotto da me.** Un test comparativo dava "tutte le forme falliscono": avevo
   fatto `cd /tmp/permtest` e `docker compose exec` non trovava più il file di compose. Il
   fallimento era del mio test, non di SQL Server. Rifatto dal repository: tutte le forme
   valide. Un test che fallisce va diagnosticato, non aggirato.

**Una scoperta collaterale che vale per la fase `C`.** Il primo test della funzione passava il
letterale `'2024-03-15'` e falliva con
`Msg 242: La conversione di un tipo di dati varchar in datetime ha generato un valore non
compreso nell'intervallo`. Causa: il login applicativo è creato con
`DEFAULT_LANGUAGE = [Italiano]`, quindi il letterale ISO viene interpretato come `gg/mm/aaaa`
(giorno 15 → mese 15 → errore). Il test ora usa `DATEFROMPARTS(2024,3,15)`.

Non è un problema del test: è **evidenza diretta per il task `C4`** (parsing di date dipendente
dalla cultura). L'applicazione non deve affidarsi a conversioni implicite, perché il risultato
cambia con la lingua del login — e il login applicativo ha una lingua diversa da `sa`.

**Evidenza.**

```
$ ./scripts/db-deploy.sh --with-seed   (con POLLING_APP_PASSWORD)   -> EXIT=0
$ ./scripts/db-deploy.sh --with-seed   (seconda esecuzione)         -> EXIT=0, autotest superato

IS_SRVROLEMEMBER('sysadmin','PollingApp')  -> 0
IS_ROLEMEMBER('db_owner','PollingApp')     -> 0  su Polling, eProcurement, Quartz
ruoli server del login                     -> (nessun ruolo)
CHECK_POLICY del login                     -> 1
login disabilitato                         -> 0  (attivo)

oggetti dbo senza alcun permesso per l'utente applicativo -> (nessuno)
      ^ il controllo di drift che rende la manutenzione del file di permessi verificata
        a ogni deploy invece che affidata alla memoria

verifica incrociata dei GRANT contro il catalogo:
  Polling GRANT=21 · eProcurement GRANT=17 · Quartz GRANT=11
  incongruenze (EXECUTE su tabelle/viste/funzioni, SELECT su procedure): nessuna
```

**Resta aperto (A3, non in questa DoD).** Nessun meccanismo di migrazione incrementale: la
modalità `apply` non è idempotente sugli oggetti già presenti. Documentato in
`sql/deploy/README.md`.

**Prossimo.** `X1.1` — layout della soluzione e `Directory.Build.props`: è il primo task del
track cross-platform, e da lì dipendono `X1.2`/`X1.3` (multi-target `net48;net8.0`).

**Blocchi.** `T0.13` in attesa della DDL dall'ambiente di origine. Nessun altro.

## 2026-10-16T15:10Z — Revisione 3 del piano (ADR 0004) — DONE

**Perché una revisione e non un'esecuzione.** `X1.1` è il primo task della fase cross-platform e
la sua DoD era **impossibile da soddisfare**:

1. chiedeva una soluzione con `PollingWorker`, `PollingApi`, `PollingWeb.Core` e `tests/*` —
   progetti **creati** da `X2.1`, `X3.1`, `X4.1` e `T1`, cioè dopo;
2. chiedeva come evidenza `dotnet build PollingWeb.sln` → **0 errori**, mentre i fatti `P13` e
   `P14` dello stesso piano dicono che la soluzione non è compilabile finché non sono fatti
   `X1.1`–`X1.3` e `X1.7`.

```
$ for p in PollingWorker PollingApi PollingWeb.Core tests; do [ -e "$p" ] && echo esiste || echo ASSENTE; done
ASSENTE (tutti e quattro)
$ dotnet build PollingWeb.sln
2 errori: MSB4249 (sito WebForms) + MSB3644 (progetti classici)
```

Un task la cui evidenza non può passare finirebbe `[x]` su un'affermazione falsa o `[~]` per
sempre, bloccando la fase: la regola 8 del prompt prescrive in questo caso una revisione con ADR.

**Decisione (ADR 0004).** `X1.1` ristretto allo scheletro di build; le richieste non ottenibili
trasferite ai task che le possono soddisfare: i progetti nella soluzione → `X2.1`/`X3.1`/`X4.1`/`T1`
(sono loro a crearli); la soluzione verde → `X1.6`/`X1.7`. Ordine rivisto:
`X1.2 → X1.3 → X1.1 → X1.4 → X1.5 → X1.6 → X1.7`. Corrette due imprecisioni: `WindowsBase` è di
`PollingClass` (quindi `X1.3`), e `X1.5` prescriveva **due** provider dati mentre ne basta uno.

---

## 2026-10-16T15:10Z — X1.2 `Database` multi-target `net48;net8.0` — DONE

**Primo task cross-platform eseguito.** Cinque interventi, tutti motivati da un fatto del piano:

| # | Intervento | Motivo |
|---|---|---|
| 1 | rimosse `FontSerif` e `Colore` e `using System.Drawing` | P6: il loro unico consumatore era `Check_RdA.cs`, rimosso in `T0.6` |
| 2 | rimosso `using System.Web` da `PollingException.cs` | non usato |
| 3 | `HttpUtility.UrlDecode` → `UrlDecodeCompat` su `Uri.UnescapeDataString` | elimina l'**ultima** dipendenza da `System.Web` nella DAL invece di aggiungere il riferimento solo per net48 (ADR 0004 §2.4) |
| 4 | `System.Data.SqlClient` → `Microsoft.Data.SqlClient` (12 file + 90 usi qualificati) | **un solo provider** per entrambi i target (ADR 0004 §1) |
| 5 | `Database.csproj` riscritto SDK-style multi-target | P14: senza questo, `MSB3644` su qualunque OS |

**Sostituzione di `HttpUtility` verificata, non assunta.** `UrlDecode` converte `+` in spazio,
`UnescapeDataString` no: la differenza è replicata. Equivalenza misurata su 8 casi, inclusi
payload di injection e UTF-8 percent-encoded → **0 differenze**. È un controllo di sicurezza:
il comportamento non doveva cambiare.

**Tre errori miei, trovati compilando.**

1. **Numerazione sfasata di uno.** `DataType.cs` inizia con `BOM + newline`, quindi la riga 1 è
   vuota e `using System.Drawing;` è la riga 2. La prima esecuzione dello script è fallita
   sull'assert invece di cancellare le righe sbagliate: l'assert ha fatto il suo lavoro.
2. **Sostituzione incompleta del provider.** Avevo cambiato solo `using System.Data.SqlClient;`
   lasciando **90 usi qualificati** (`System.Data.SqlClient.SqlCommand`, `.SqlDataAdapter`): la
   build net8.0 ha fallito con `CS1069`. Verificato prima di sostituire che fossero tutti codice
   e nessuno una stringa (avrebbe rotto il `providerName` della configurazione).
3. **Riferimento mancante per net48.** Il progetto SDK-style non aggiunge
   `System.ComponentModel.DataAnnotations`, implicito nel progetto classico → `CS0234`. Aggiunto
   con `Condition="'$(TargetFramework)' == 'net48'"`, perché su net8.0 appartiene al framework
   condiviso.

**Evidenza.**

```
  net48   errori=0  avvisi=0
  net8.0  errori=0  avvisi=28   (tutti CA2200, cioè `throw ex;`)
  artefatti: Database/bin/Debug/net48/Database.dll
             Database/bin/Debug/net8.0/Database.dll

  System.Web o System.Drawing in Database : 0
  System.Data.SqlClient in Database       : 0
  TFM dichiarati                          : net48;net8.0
  Database.csproj non è più un csproj classico

  dotnet build PollingWeb.sln
    prima:  MSB4249 (sito) + MSB3644 (progetti classici)
    dopo:   2 × MSB4249 (solo il sito)     <- i MSB3644 sono spariti con X1.2
  dotnet build PollingClass/PollingClass.csproj -> MSB3644 invariato (è X1.3)
```

**Il numero 28 non è rumore: è la misura di `C6`.** Gli avvisi `CA2200` sono
`throw ex;`, che azzera lo stack trace. Il piano lo prevedeva come elenco qualitativo; ora ha una
cifra: **28 occorrenze in `Database`** (0 in `PollingClass`).

**`SYSLIB0051` soppresso e documentato.** Proviene solo da `dtsTabelle.Designer.cs`, il DataSet
generato dal designer, che usa i costruttori di serializzazione basati su formatter. La
soppressione è di progetto e commentata nel csproj; il ridisegno del livello dati è `C2`. Ho
provato prima la soppressione per-file (`<Compile Update>` + `NoWarn`): non ha effetto sugli
avvisi del compilatore, quindi ho cambiato approccio invece di lasciarla lì a sembrare utile.

**Nuovo fatto registrato: P15.** Il cambio di provider ha una conseguenza non ovvia, verificata
sul container:

```
  FALLITA  default (Encrypt non specificato) -> SqlException: errore durante l'handshake pre-login
  OK       TrustServerCertificate=True      -> Polling
  OK       Encrypt=False                    -> Polling
```

`Microsoft.Data.SqlClient` 5.x attiva `Encrypt` per default. Le connection string servono
`TrustServerCertificate=True` (o un certificato valido). Colpisce la quickstart (**AC8**) e va
intercettato con un messaggio comprensibile da `C7`: l'errore predefinito non dice cosa manca.

**Prossimo.** `X1.3` — `PollingClass` multi-target: rimozione di `LinqToExcel`/`Remotion` (LGPL,
Windows-only), estrazione della logica dei 4 ASMX e isolamento di `Prog_Quartz`.

**Blocchi.** `T0.13` in attesa della DDL dall'ambiente di origine. Nessun altro.

## 2026-10-16T16:00Z — Revisione 4 del piano (ADR 0005) — DONE

**Misurato prima di eseguire.** `X1.3` chiedeva un `PollingClass` multi-target `net48;net8.0`.
Prima di provarci ho contato i file non compilabili per net8:

| File | Righe | Quartz 2.x | `System.Web.Services` |
|------|------:|-----------:|----------------------:|
| `Ordini_Sap.cs` | 721 | 34 | 8 |
| `ImportFatture.cs` | 621 | 25 | 4 |
| `Import_Budget.cs` | 478 | 21 | 3 |
| `ExportSQL01.cs` | 449 | 20 | — |
| `LeggiFornitore.cs` | 381 | — | 4 |
| `Prog_Quartz.cs` | 255 | 25 | — |
| **totale** | **2 905** | | |
| **`PollingClass`** | **5 246** | | |

**Il 55% dell'assembly non è compilabile per net8** — e sono esattamente i flussi di dominio:
invio ordini, import budget, import fatture, lettura fornitori, export. Un `net8.0` ottenuto
escludendoli passerebbe l'evidenza «compila» mentre l'assembly non conterrebbe il motore:
sarebbe un'affermazione falsa sulla portabilità.

**Decisione (ADR 0005).** `X1.3` ristretto alla parte verificabile; il multi-target scomposto in
`X1.3b` (estrazione della logica dai 4 ASMX — **dopo `T1`**, perché sono 2 201 righe stateful e
oggi non esiste alcun test: un refactoring così senza rete di sicurezza rompe il sistema in modo
non rilevabile) e `X1.3c` (**dopo `X1.3b` e `X2.2`**: con Quartz 2.x il 55% del codice non può
stare nel target net8).

**Regola anti-svuotamento**, introdotta perché il problema appena misurato si ripresenti in
forma peggiore: da `X1.3c` un task di multi-target deve riportare righe compilate per target,
rapporto `righe(net8)/righe(totali)` con **soglia ≥ 95%** e l'elenco esplicito delle esclusioni.
Senza, «multi-target» sarebbe soddisfacibile con una lista di esclusione.

---

## 2026-10-16T16:00Z — X1.3 `PollingClass` SDK-style net48 + rimozione LGPL — DONE

**Risultato principale: `PollingClass` si compila su Linux.** Prima era impossibile
(`MSB3644`: csproj classico su `v4.5` senza targeting pack), quindi nessun task successivo era
verificabile su questa macchina.

**Modifiche.**

- `PollingClass.csproj` riscritto SDK-style (`net48`, `Microsoft.NETFramework.ReferenceAssemblies`);
  Quartz e Common.Logging ora da NuGet invece dei `HintPath` verso `PollingWeb/Bin`;
  `WindowsBase` rimosso (non usato: 0 occorrenze di `System.Windows`/`Dispatcher`).
- **`LinqToExcel` e `Remotion` rimossi**: l'unica dipendenza copyleft del progetto era anche
  **codice morto** — `ConnexionExcel` era usata solo in un commento di `Import_Budget.cs`.
  `ImportaExcel.cs` e `PollingClass/packages.config` eliminati; le 5 DLL/XML LGPL rimosse da
  `PollingWeb/Bin`; `NOTICE` e `SECURITY.md` aggiornati da «da rimuovere» a «rimossa».
- 11 file passati a `Microsoft.Data.SqlClient` (provider unico, ADR 0004 §1).

**Un errore mio evitato per un soffio.** Avevo concluso che `ImportaExcel.cs` fosse
interamente morto e l'avevo cancellato. La build ha risposto `CS0246: 'Budget' non trovato`:
il file conteneva **due** classi — `ConnexionExcel` (LGPL, morta) **e** `Budget` (usata da
`ImportaXML`). Il mio controllo era troppo grossolano: avevo cercato i nomi della *classe* e del
*file*, non le classi in esso definite. `Budget` è stata estratta in `PollingClass/Budget.cs`
(classe di soli dati, platform-neutral) e documentata come tipizzata per il parser di budget.

**Evidenza.**

```
$ dotnet build PollingClass/PollingClass.csproj -f net48
Compilazione completata.  Avvisi: 5  Errori: 0
      ^ 5 avvisi preesistenti: variabili e campi assegnati ma mai usati

$ git grep -inE 'remotion|linqtoexcel' -- '*.cs' '*.csproj' '*.config' '*.props' '*.targets' '*.sln' '*.sqlproj'
(nessun risultato)                     # 0 riferimenti in codice, progetti e configurazione

  WindowsBase nel csproj              : 0
  HintPath verso PollingWeb/Bin       : 0
  System.Data.SqlClient in PollingClass: 0
  pacchetti ripristinati              : Quartz 2.1.2, Common.Logging 2.1.2,
                                        DocumentFormat.OpenXml 2.7.1, Microsoft.Data.SqlClient 5.2.2,
                                        System.Configuration.ConfigurationManager 8.0.0 (+ transitivi)

$ rm -rf PollingClass/bin PollingClass/obj     # artefatti del vecchio progetto, con le DLL LGPL
```

**Perché l'evidenza sul grep è limitata a codice/progetti/configurazione.** `NOTICE`,
`SECURITY.md`, il piano e questo registro **devono** nominare la dipendenza rimossa: documentarne
la rimozione fa parte del compito. Un `git grep` «= 0» su tutto l'albero sarebbe una verifica
impossibile, quindi falsa. La restrizione è dichiarata nella DoD.

**Prossimo secondo l'ordine rivisto.** `T1` — infrastruttura di test (xUnit, container SQL Server,
`db-deploy.sh` nei test). È il prerequisito di `X1.3b`: senza test, il refactoring dei 4 ASMX non
è verificabile.

**Blocchi.** `T0.13` in attesa della DDL dall'ambiente di origine. Nessun altro.

## 2026-10-16T17:00Z — T1 Infrastruttura di test — DONE

**Vincolo verificato prima di scrivere i test.** I test devono essere **eseguiti**, e su questa
macchina esistono solo i runtime .NET 8 e .NET 10:

```
$ dotnet --list-runtimes
  Microsoft.NETCore.App 8.0.32 / 10.0.13
  Microsoft.AspNetCore.App 8.0.32 / 10.0.13
$ which mono
  (assente)
```

Quindi un progetto di test `net48` si **compilerebbe** ma non girerebbe: sarebbe un test
inutile in CI. I due progetti di test hanno target **`net8.0`**, e questo ha una conseguenza
che va dichiarata: `Database` è multi-target e si testa; **`PollingClass` no** (è `net48` fino
a `X1.3c`), quindi parser, flussi e wrapper di stored procedure **restano scoperti**. È scritto
in `tests/README.md` e nella DoD, non nascosto.

**Due progetti, 66 test reali.**

| Progetto | Test | Copre |
|---|---|---|
| `PollingWeb.UnitTests` | 50 superati, 1 ignorato | `AntiSqlInject`, `ValidaData`, `ValidaMail`, `IsNumeric` |
| `PollingWeb.IntegrationTests` | 16 superati | contratto dello schema + privilegi minimi |

Non sono test di riempimento:

- i **6 test su `AntiSqlInject`** fissano la modifica di `X1.2`: lì ho sostituito
  `HttpUtility.UrlDecode` asserendo l'equivalenza su 8 casi. Ora quell'asserzione è un test, e
  una futura modifica alla decodifica non può passare inosservata;
- i **16 test di integrazione** danno valore operativo a `T0.7`, `T0.8` e `T0.12`: numeri di
  tabelle/viste/funzioni/procedure/chiavi esterne per i tre database, la collocazione delle due
  tabelle spostate, la semantica fuorviante di `Datini`/`Datfin`, e **4 test negativi** che
  verificano che l'utente applicativo **non** possa eseguire DDL né essere `db_owner`/`sysadmin`.

**Errore mio, corretto osservando invece di indovinare.** Il primo test su `GetSafetySql`
asseriva che il sanificatore restituisse stringa vuota. **Falso.** Ho scritto un probe,
osservato il comportamento e riscritto:

```
  in=' OR 1=1 --          safe=False out=' 1=1 '        out_safe=True
  in=1' or '1'='1         safe=False out='1 1=1'        out_safe=True
  in=x; drop table tool   safe=False out='x tool'       out_safe=True
  in=SELECT * FROM utenti safe=False out='utenti'       out_safe=True
  in=Fornitore Sin. SRL   safe=True  out='Fornitore Sintetico SRL'
  in=sconto 50%           safe=False out='sconto 50'    out_safe=True
  in=codice A#1           safe=False out='codice a1'    out_safe=True
```

Da lì sono venuti tre test migliori di quello che avevo scritto a intuito: la **proprietà
forte** `IsSafetySql(GetSafetySql(x)) == true` per ogni payload, un test di regressione sui
valori esatti, e un test che **documenta un difetto reale**: il filtraggio rimuove `%` e `#`
anche da testo legittimo («sconto 50%» → «sconto 50»), e abbassa il caso. Su tracciati SAP con
percentuali è un rischio funzionale: è materia di `C1`–`C3`.

**Due difetti documentati dai test, non approvati.**

- `ValidaData` dipende dalla cultura del thread: `"31/12/2024"` è valido con cultura italiana e
  **rifiutato** con cultura statunitense. Il test asserisce entrambi i comportamenti e si
  chiama `..._difetto_C4`. Quando `C4` renderà il parsing indipendente dalla cultura, il test
  dovrà essere aggiornato: è il segnale che si vuole.
- `ValidaMail.IsValidEmail(null)` genera `NullReferenceException` perché `email.Replace(...)`
  è **fuori** dal blocco `try`. Il test è marcato `Skip` con la motivazione e l'asserzione del
  comportamento voluto: quando `C6` lo corregge, basta togliere lo `Skip`.

**Salto automatico, verificato in entrambe le direzioni.**

```
$ POLLING_DB_$1=[REDACTED] dotnet test tests/PollingWeb.IntegrationTests
  Superato!  - Non superati: 0. Superati: 16. Ignorati: 0.  Totale: 16
$ dotnet test tests/PollingWeb.IntegrationTests        # senza credenziali
  Ignorato!  - Non superati: 0. Superati: 0. Ignorati: 16. Totale: 16
```

Un test di integrazione che fallisce perché manca il database non segnala un difetto del codice,
e un fallimento che si impara a ignorare è peggio di un test assente.

**`scripts/test-integration.sh`** incatena le tre cose nell'ordine in cui le farebbe una persona:
avvia il container, applica lo schema con `scripts/db-deploy.sh` (come richiede la DoD), lancia
i test. Eseguito end-to-end: container già attivo → deploy `recreate` con seed → autotest dei
permessi superato → 16 test superati.

**Errore XML, lo stesso di `T0.2`.** Il commento nel `.csproj` conteneva `--with-seed` e due
trattini consecutivi non sono ammessi in un commento XML: `MSB4025`. Corretto riformulando. È la
seconda volta: vale la pena ricordarlo, e l'ho annotato nel file stesso.

**Limite dichiarato.** `dotnet test PollingWeb.sln` non è ancora possibile: la soluzione contiene
il sito WebForms, non compilabile in CI (`MSB4249`, fatto `P13`). I due progetti si lanciano
singolarmente; l'esecuzione unificata arriva con `X1.7`.

**Prossimo.** `X1.3b1` — separazione Quartz in `Fornitori_Scheduler`.

**Blocchi.** `T0.13` in attesa della DDL dall'ambiente di origine.

---

## 2026-10-16T16:xxZ — X1.3b1 Separazione Fornitori_Scheduler/Quartz — DONE

**Modifica.** Estratta la policy di composizione cron in `PollingClass/FornitoriSchedulePolicy.cs`; introdotto `IFornitoriSchedulingGateway` e implementazione Quartz 2.x in `PollingClass/FornitoriQuartzGateway.cs`; spostato il job Quartz in `PollingClass/JobFornitori.cs`. `Fornitori_Scheduler` conserva le firme esistenti e delega al gateway (costruttore parametrico disponibile per sostituzione/test). La policy conserva la forma cron esistente; l'adapter Quartz conserva identità, mappa dati, override e scheduling one-time/interval. Aggiunti test unitari per cron e range/validazione.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
Compilazione completata. Avvisi: 5. Errori: 0.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 65. Ignorati: 1. Totale: 66.
$ grep -nE '\\b(Quartz|IJob|StdSchedulerFactory|JobBuilder|TriggerBuilder|IScheduler)\\b' PollingClass/Prog_Quartz.cs
(nessuna corrispondenza)
$ git diff --check
(exit 0)
```

**Limiti.** `PollingClass` resta `net48`; la build è stata eseguita su Linux con reference assemblies. Windows/macOS non verificati in locale. `PollingWeb.sln` non è stata compilata: il Website WebForms resta un blocker noto. La scansione dei 16 pattern riservati ha dato 0 match nei file tracciati e 0 nei blob raggiungibili.

**Prossimo task:** `X1.3b3 — ImportBudget_Scheduler`.

---

## 2026-10-16T16:xxZ — X1.3b3 Separazione ImportBudget_Scheduler/Quartz — DONE

**Modifica.** `ImportBudget_Scheduler` mantiene firma e logica di caricamento parametri, delegando FirstStart e Polling_Interval a `IFornitoriSchedulingGateway`. Aggiunti `PollingClass/ImportBudgetQuartzGateway.cs` (Quartz 2.x) e `PollingClass/JobBudget.cs`; `Import_Budget.cs` non contiene più simboli Quartz. L'adapter mantiene il trigger feriale lun-ven, identity legacy, flag `onetime`, chiamata a `ImportBudget.Leggi_File(..., onetime)`, override e scheduling immediato/intervallo. La chiamata esistente `TaskServiceClass.LeggiParametri` nel percorso interval resta mantenuta.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
Compilazione completata. Avvisi: 5. Errori: 0.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 80. Ignorati: 1. Totale: 81.
$ grep -nE '\\b(Quartz|IJob|StdSchedulerFactory|JobBuilder|TriggerBuilder|IScheduler)\\b' PollingClass/Import_Budget.cs
(nessuna corrispondenza)
$ git diff --check
(exit 0)
```

**Limiti.** Target `PollingClass` resta net48; build eseguita solo su Linux. Windows/macOS non verificati localmente. Non compilata l'intera soluzione WebForms per il blocker noto.

**Prossimo task:** `X1.3b5 — SQL01_Scheduler`.

---

## 2026-10-16T17:xxZ — X1.3b5 Separazione SQL01_Scheduler/Quartz — DONE

**Modifica.** `SQL01_Scheduler` mantiene le firme e la lettura dei parametri, delegando FirstStart e Polling_Interval a `IFornitoriSchedulingGateway`. Aggiunti `PollingClass/SQL01QuartzGateway.cs` e `PollingClass/JobSQL01.cs`; rimossi i riferimenti Quartz da `ExportSQL01.cs`. Il job conserva l'entry point `ExportSQL01.Leggi_File(..., onetime)` e il gateway mantiene identità, JobDataMap, cron giornaliero configurato, override e schedulazione interval/immediata.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
Compilazione completata. Avvisi: 5. Errori: 0.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 97. Ignorati: 1. Totale: 98.
$ grep -nE '\\b(Quartz|IJob|StdSchedulerFactory|JobBuilder|TriggerBuilder|IScheduler)\\b' PollingClass/ExportSQL01.cs
(nessuna corrispondenza)
$ git diff --check
(exit 0)
```

**Limiti.** `PollingClass` resta net48; verificata solo la build Linux. Windows/macOS non verificati localmente; la soluzione WebForms non è stata compilata.

**Prossimo task:** verificare/chiudere `X2.2 — Quartz 3.x con job store ADO su SQL Server`.

---

## 2026-10-16T17:xxZ — X2.2 Quartz 3.13.1 con job store ADO — DONE

**Modifiche.** Quartz 2.1.2 aggiornato a 3.13.1; aggiunto `Quartz.Serialization.Json`. `QuartzSchedulerFactory` configura `JobStoreTX`, `SqlServerDelegate`, provider `MicrosoftDataSqlClient`, prefisso `QRTZ_`, JSON serializer, misfire threshold 60 s e clustering disabilitato. Connection string da parametro, `POLLING_QUARTZ_CONNECTION` o `QuartzConnectionString`; nessun segreto in config/version control. I gateway usano la factory condivisa; le API asincrone Quartz sono attese nei boundary legacy, `IJob.Execute` restituisce `Task`. Il job/payload storico inserito nello schema è stato rimosso: deploy ora contiene solo DDL. Schema SQL mantenuto sulle 11 tabelle QRTZ e permessi minimi.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet clean PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
(exit 0)
$ dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
Compilazione completata. Avvisi: 5. Errori: 0.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 98. Ignorati: 2. Totale: 100.
$ POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] ./scripts/test-integration.sh
Deploy SQL Server + autotest permessi; Quartz_3_persistent_store_restarts_with_trigger_state_preserved PASS;
Totale test: 16; Superati: 16; exit 0.
$ dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj # senza credenziali
Totale test: 16; Ignorati: 16; exit 0.
```

Il test persistente crea job/trigger sintetici sul DB Quartz, riavvia lo scheduler con lo stesso nome e verifica il recupero dallo store SQL, poi pulisce i record. L'esecuzione passa dal test d'integrazione (`LeastPrivilegeTests`), non dalla suite unit senza connessione. Nessuna immagine Docker aggiunta. Windows/macOS non verificati localmente; la soluzione WebForms non compilata. Decisioni/configuration sono registrate in `docs/adr/0007-quartz-3-persistent-store.md`.

**Prossimo task:** `X1.1 — Layout soluzione + Directory.Build.props`.

---

## 2026-10-16T18:40Z — X1.1 Layout props/package management — DONE

**Problema riprodotto / atteso.** Prima: i tre file di build centralizzati non esistevano; LangVersion/Nullable e versioni dei pacchetti erano duplicate nei csproj. Dopo: le proprietà e versioni devono derivare dai file root; restore/build dei progetti SDK deve restare funzionante; la soluzione deve elencare solo i progetti effettivamente esistenti.

**Modifiche.** Aggiunti `Directory.Build.props` (LangVersion, Nullable, Deterministic, Version), `Directory.Packages.props` (Central Package Management per i 9 package ID effettivamente usati) e `Directory.Build.targets` (ContinuousIntegrationBuild quando `CI=true`). Rimossi i valori duplicati dai quattro csproj SDK; aggiornato il moniker Website in soluzione a .NET Framework 4.8; allineato il commento test al multi-target già disponibile; escluso `.codegraph/` locale da Gitignore.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet msbuild Database/Database.csproj -getProperty:LangVersion -getProperty:Version -getProperty:Nullable -getProperty:Deterministic
LangVersion=7.3; Version=1.0.0; Nullable=disable; Deterministic=true
$ dotnet sln PollingWeb.sln list
Database, PollingClass, PollingWeb (Website), PollingWeb.IntegrationTests, PollingWeb.UnitTests
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
Tutte le quattro build: exit 0, 0 errori, 0 avvisi.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
98 passati, 2 ignorati, 0 falliti.
$ dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet
16 ignorati, 0 falliti (DB non configurato in questa esecuzione).
$ dotnet restore PollingWeb.sln --nologo
exit 1: MSB4249 per il Website WebForms, limite noto pre-X1.7.
$ git diff --check
exit 0.
```

La soluzione contiene i progetti esistenti: i due classici test e le due librerie, oltre al sito Website legacy. Non si dichiara build verde dell'intera soluzione; Windows/macOS non verificati localmente.

**Prossimo task:** `X1.4 — Configurazione astratta`.

---

## 2026-10-16T19:xxZ — X1.4 Configurazione astratta — DONE

**Riproduzione baseline.** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1, MSB4249 (Website WebForms). `rg ConfigurationManager Database PollingClass --glob '*.cs'` mostrava accessi diretti in `DatabaseInfo.cs`, `QuartzSchedulerFactory.cs`, `MailSede.cs` (4 chiamate). Evidenza attesa: astrazione comune, provider fake, JSON/environment per net8, fallback legacy net48 e accesso a `ConfigurationManager` confinato all'adapter.

**Modifiche.** Aggiunti `Database/AppConfig.cs` con `IAppConfig`, implementazione `AppConfig` che legge `IConfiguration` (appsettings.json + environment variables) o valori forniti dal chiamante e fallback `ConfigurationManager`; `DatabaseInfo`, `QuartzSchedulerFactory` e `MailSede` consumano l'interfaccia/provider. Test fake e test `IConfiguration` in `tests/PollingWeb.UnitTests/AppConfigTests.cs`. Ninguna logica di dominio cambiata.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori (29 avvisi CA2200 esistenti).
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori, 0 avvisi.
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori, 0 avvisi.
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori, 6 avvisi.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
101 passati, 2 ignorati, 0 falliti.
$ dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet
16 ignorati, 0 falliti (istanza DB non configurata in questa esecuzione).
$ rg -n 'ConfigurationManager' Database PollingClass --glob '*.cs'
solo 2 match nell'adapter Database/AppConfig.cs.
$ git diff --check
exit 0.
```

La soluzione completa non è buildata per il blocker Website MSB4249 riprodotto; Windows/macOS non verificati localmente.

**Prossimo task:** `X1.5 — Provider dati astratto (Microsoft.Data.SqlClient)`.

---

## 2026-10-16T19:40Z — X1.5 Provider dati Microsoft.Data.SqlClient — DONE

**Baseline riprodotto.** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1, `MSB4249` Website WebForms. Verifica sorgente: 146 riferimenti/costruttori ADO netti nel DAL, tutti `Microsoft.Data.SqlClient`; `System.Data.SqlClient` non trovato. Il provider è già unico; il gap era l'assenza di un seam factory esplicito e di un test CRUD contro SQL Server.

**Modifiche.** Aggiunti `Database/ISqlClientFactory.cs` e implementazione `MicrosoftSqlClientFactory`; `TableAdapter.BuildCommand` usa l'astrazione per creare `SqlCommandBuilder`. Aggiunto test d'integrazione `SqlClientAdapterTests` che crea una tabella temporanea e testa insert/update/delete via `SqlDataAdapter`/`SqlCommandBuilder`, rimuovendo sempre la fixture. Nessun seed/dato reale.

**Evidenza reale (Linux, 2026-10-16).**

```
$ POLLING_DB_PASSWORD="(presa dall'ambiente runtime SQL locale)" ./scripts/db-deploy.sh --with-seed
Deploy schema Polling/eProcurement/Quartz e seed sintetico completato; passaggi account app saltati senza POLLING_APP_PASSWORD.
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --nologo -v quiet
exit 0; 0 errori.
$ POLLING_DB_PASSWORD="(valore locale non registrato)" dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet
8 passati, 9 ignorati, 0 falliti; test SqlClientAdapterTests incluso nel conteggio PASS.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
101 passati, 2 ignorati, 0 falliti.
$ rg -n 'System.Data.SqlClient' Database PollingClass --glob '*.cs' --glob '*.csproj'
nessuna corrispondenza; 104 match Microsoft.Data.SqlClient in Database.
$ git diff --check
exit 0.
```

Il login applicativo non è configurato, quindi i 9 test che lo richiedono sono ignorati; Windows/macOS non verificati localmente. La build della soluzione non è verificata verde (MSB4249 Website).

**Prossimo task:** `X1.6 — CI multi-OS`.

---

## 2026-10-16T20:00Z — X1.6 CI multi-OS — DONE

**Baseline.** `.github/workflows/ci.yml`, `actionlint`, `act` e `yamllint` assenti. Evidenza attesa: workflow valido con matrix Ubuntu/macOS/Windows, build entrambi i TFM delle librerie, test unit, integration SQL Server Ubuntu.

**Modifica.** Aggiunto `.github/workflows/ci.yml`: job build/test su `ubuntu-latest`, `macos-latest`, `windows-latest`, setup .NET 8, restore, build Database/PollingClass `net48` + `net8.0` e unit tests. Job SQL Server su Ubuntu usa il service container e i secrets configurati per il deploy/test; il job è condizionato alla presenza delle due credenziali necessarie, senza fallback hardcoded.

**Evidenza reale (Linux, 2026-10-16).**

```
$ python3 -c 'import yaml; d=yaml.safe_load(open(".github/workflows/ci.yml")); print(d["jobs"]["build-and-test"]["strategy"]["matrix"]["os"]); print(list(d["jobs"]))'
YAML parse OK; matrix [ubuntu-latest, macos-latest, windows-latest]; jobs build-and-test, integration.
$ dotnet restore tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj
exit 0.
$ dotnet build Database/Database.csproj -f net48 --no-restore --configuration Release
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --configuration Release
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --configuration Release
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --configuration Release
Tutte le build: exit 0, 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --configuration Release
101 passati, 2 ignorati, 0 falliti.
$ dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --configuration Release
exit 0, 0 errori.
$ dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --configuration Release
17 ignorati, 0 falliti (nessuna credenziale DB esposta nell'ambiente di questa verifica).
$ git diff --check
exit 0.
```

Non eseguiti `act`/`actionlint` né run remoto; macOS e Windows non verificati localmente, la CI multi-OS va confermata dal run GitHub successivo.

**Prossimo task:** `X1.7 — Website project legacy fuori dal build CI`.

---

## 2026-10-16T20:30Z — X1.7 Website legacy escluso dal build default — DONE

**Baseline riprodotto.** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1 con `MSB4249`: il compilatore ASP.NET Website WebForms non è disponibile in MSBuild cross-platform. Evidenza attesa: dichiarare il Website come legacy opzionale, descrivere l'ambiente di compilazione su richiesta e assicurare che il workflow predefinito lavori sui progetti SDK invece di tentare la soluzione Website.

**Modifica.** Aggiunto `docs/LEGACY.md` con motivazione, limiti e procedura di compilazione IIS su Windows tramite Visual Studio/.NET Framework 4.8 Developer Pack. Il workflow CI introdotto in X1.6 compila soltanto i progetti SDK cross-platform e non invoca `PollingWeb.sln`; nessuna compilazione legacy è stata simulata o dichiarata completata.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingWeb.sln --nologo -v quiet
exit 1: MSB4249 sul Website WebForms.
$ grep -n 'dotnet build PollingWeb.sln' .github/workflows/ci.yml
nessuna corrispondenza; workflow build/test per i progetti SDK soltanto.
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
101 passati, 2 ignorati, 0 falliti.
$ test -f docs/LEGACY.md
exit 0.
$ git diff --check
exit 0.
```

Windows/macOS e compilazione IIS legacy non verificati localmente. La soluzione Visual Studio continua a mostrare il Website per gli sviluppatori legacy; non si asserisce che la solution build sia verde.

**Prossimo task:** `C1 — FiltroCampi.BuildWhere parametrizzato`.

---

## 2026-10-16T21:00Z — C1 FiltroCampi.BuildWhere parametrizzato — DONE

**Baseline riprodotto.** `Database/DataType.cs` concatenava campo, operatore e valori nella stringa SQL (`BuildWhere(ArrayList)`); 18 call site dei table adapter. Campo funzionale calcolato esistente `isnull(Elaborato,0)` è ora riconosciuto esplicitamente, senza autorizzare espressioni arbitrarie.

**Modifiche.** Nuovo `BuildWhere(ArrayList, SqlCommand)`: allowlist dei campi impiegati dai filtri del DAL, operatori `= < > <= >=`, connettori AND/OR e parentesi; tipi C/N/D validati, ogni valore è parameterizzato (`@pN`), numeri/date convertiti invariant. Il factory helper del `TableAdapter` pulisce i parametri e li allega al `SelectCommand`; tutti i 18 call site in 9 table adapter aggiornati. Overload string mantenuto marcato Obsolete per compatibilità, da rimuovere nel task C2; nessun call site SQL DAL lo usa più. Aggiunti test di injection, input non ammessi e conversioni numerico/data.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
104 passati, 2 ignorati, 0 falliti.
$ dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet
17 ignorati, 0 falliti (istanza DB non configurata per questa verifica).
$ rg -n 'FiltroCampi.BuildWhere' Database/TableAdapter*.cs
solo il wrapper helper parametrizzato in TableAdapter.cs; tutti i consumer usano `this.BuildWhere(array)`.
$ git diff --check
exit 0.
```

Il test SQL Server injection/assenza di righe extra non è eseguito; i test unit confermano che il payload resta un parametro e non entra nel testo SQL. Windows/macOS non verificati localmente.

**Prossimo task:** `C2 — Migrazione di tutti i TableAdapter`.

---

## 2026-10-17T09:00Z — C2 Migrazione TableAdapter — DONE

**Baseline riprodotto.** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1, `MSB4249` Website legacy. I 18 call site dei table adapter delegavano già la creazione dei parametri ma interpolavano la clausola nel `CommandText`; esisteva ancora un overload string `BuildWhere(ArrayList)` che convertiva parametri in SQL literal. Esistevano anche overload `Load(string,string,string)` che accettavano raw SQL, senza call site nel repository.

**Modifiche.** Tutti i 18 load (9 adapter) ora svuotano i parametri del proprio `SelectCommand`, chiamano `BuildWhere(ArrayList, SqlCommand)` e fissano il testo query con placeholder. Eliminati l'overload `BuildWhere(ArrayList)` con interpolazione SQL e i 9 overload raw `Load(...)` non usati. Helper `BuildWhereClause` centralizza assemblaggio testo + parametri. Aggiunto test senza DB che verifica il comando prodotto mantiene `' OR 1=1 --` solo in `SqlParameter`; il test SQL adapter CRUD preesistente esegue insert/update/delete su tabella sintetica temporanea.

**Evidenza reale (Linux, 2026-10-17).**

```
$ dotnet build PollingWeb.sln --nologo -v quiet
exit 1: MSB4249 Website WebForms.
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
104 passati, 2 ignorati, 0 falliti.
$ POLLING_DB_PASSWORD="(letto localmente dal runtime SQL, non registrato)" dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet --filter FullyQualifiedName~SqlClientAdapterTests
2 passati, 0 ignorati, 0 falliti (container SQL Server locale).
$ rg -n 'BuildWhere\\(ArrayList filtroPar\\)|public int Load\\(string tabelle' Database --glob '*.cs'
nessuna corrispondenza.
$ git diff --check
exit 0.
```

I test d'integrazione verificano fixture SQL sintetica CRUD e il CommandText/parameter binding del payload. Windows/macOS non verificati localmente.

**Prossimo task:** `C3 — Rimozione concatenazione in SqlDataSource/pagine`.

---

## 2026-10-17T09:30Z — C3 Query parametrizzate in TaskService WebForms — DONE

**Baseline riprodotto.** `rg` trovava `TaskService.aspx.cs` con `SelectCommand = "..." + ddlIdProcessi.SelectedValue + ...`, che interpolava input del controllo nel SQL. Le query `InsertCommand`/`UpdateCommand` usano già placeholder `@...` e `Parameter` bound; le query markup sono statiche.

**Modifica.** Il lookup ora imposta `SELECT * FROM DimProcessi WHERE IdProcesso = @IdProcesso`, pulisce `SelectParameters` e aggiunge il valore mediante parametro stringa. Nessun cambiamento di contratto o logica funzionale.

**Evidenza reale (Linux, 2026-10-17).**

```
$ rg -n 'SelectCommand\\s*=.*\\+|SelectCommand.*SelectedValue|\\+.*ddl.*SelectedValue' PollingWeb --glob '*.cs' --glob '*.aspx'
nessuna interpolazione SQL con SelectedValue; resta solo l'uso di SelectedValue per identità Quartz, non per query.
$ rg -n 'InsertCommand\\s*=.*\\+|UpdateCommand\\s*=.*\\+' PollingWeb --glob '*.cs'
solo concatenazione di frammenti SQL statici; valori sono placeholder con Parameter bound.
$ dotnet build PollingWeb.sln --nologo -v quiet
exit 1: MSB4249 Website WebForms (blocker noto).
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
104 passati, 2 ignorati, 0 falliti.
$ git diff --check
exit 0.
```

La pagina WebForms non può essere compilata/eseguita dai test net8; il test pagina è bloccato dal Website legacy MSB4249. Windows/macOS non verificati localmente.

**Prossimo task:** `C4 — Eliminazione Convert.ToDateTime(string) culture-dependent`.

---

## 2026-10-17T10:00Z — C4 Parsing date invariant — DONE

**Baseline riprodotto.** `Convert.ToDateTime(string)` era usato da parser `ValidaData`, table adapter e policy cron; il test preesistente confermava che `31/12/2024` passava con `it-IT` e falliva con `en-US`.

**Modifiche.** `ValidaData` ora usa `DateTime.TryParseExact` con formati espliciti e `InvariantCulture`; FEvalore formata ISO invariant. `IsNumeric` usa `double.TryParse` invariant e output numerico invariant. I TableAdapter leggono i campi data da `SqlDataReader.GetDateTime`; schedulatori usano `DataRow` DateTime typed e serializzano numeri cron invariant. Date SAP ricevute come `DateTime` nel parser sono formattate direttamente con invariant. Aggiornati test cultura: stessi input/esiti per `en-US` e `it-IT`.

**Evidenza reale (Linux, 2026-10-17).**

```
$ rg -n 'Convert\\.ToDateTime\\(' Database PollingClass PollingWeb --glob '*.cs' | grep -v '//'
nessuna conversione eseguibile (solo commento storico in ExportSQL01.cs).
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
106 passati, 2 ignorati, 0 falliti.
$ git diff --check
exit 0.
```

Windows/macOS non verificati localmente; WebForms completo non compilabile su Linux (MSB4249).

**Prossimo task:** `C5 — DatabaseInfo thread-safe, senza stato statico mutabile`.

---

## 2026-10-17T10:30Z — C5 DatabaseInfo thread-safe — DONE

**Baseline riprodotto.** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1/MSB4249, Website legacy. `DatabaseInfo` esponeva campi/statiche di configurazione condivisi e `TableAdapter.BuildCommand` vi accedeva per il timeout; source grep trovava 4 accessi statici al timeout.

**Modifiche.** Riscritto `DatabaseInfo` come oggetto per-istanza: `IAppConfig` iniettato al costruttore, connessione dedicata, timeout command per istanza con default 30 s; rimosse configurazioni e stringhe di connessione statiche mutabili. `Dispose` chiude connection/transaction e prova rollback transaction attiva. `TableAdapter.BuildCommand` non legge più timeout globale: conserva il timeout del command/default sicuro. Test aggiunti per 100 istanze parallele con connection strings/timeouts diversi e fallback invalid/missing.

**Evidenza reale (Linux, 2026-10-17).**

```
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
108 passati, 2 ignorati, 0 falliti.
$ rg -n 'DatabaseInfo\\.(timeOut|StringaConnessione|StringaConnessioneEproc|data_source|user_id|password|initial_catalog|AppConfigProvider|Configure)' Database --glob '*.cs'
nessuna corrispondenza (nessun accesso statico configurazione).
$ git diff --check
exit 0.
```

`dotnet build PollingWeb.sln` resta bloccato dal Website legacy MSB4249; Windows/macOS non verificati localmente.

**Prossimo task:** `C6 — Gestione errori e logging`.

---

## 2026-10-17T11:00Z — C6 Error handling e logging — DONE

**Baseline riprodotto.** Scansioni su sorgenti iniziali: 28 `throw ex;` in DAL, catch vuoti nei helper, 14 `Console.WriteLine(ex.Message)` nei wrapper/domain. `Aggset` costruiva nuove eccezioni includendo SQL completo e messaggio del driver nel testo dell'errore.

**Modifiche.** I TableAdapter e `Aggset` rilanciano con `throw;`, preservando stack; catch che fanno rollback/reject mantengono la recovery e rilanciano senza alterazione. `TableAdapter.BuildCommand` non stampa più né sopprime errori: l'errore di `SqlCommandBuilder` propaga. I catch vuoti nei helper sono stati sostituiti da fallback espliciti o gestione di eccezioni specifiche. I wrapper che stampavano su console inoltrano l'errore all'handler; `PollingExceptionHandler` emette `Trace.TraceError` con tipo e dettagli exception. `Aggset` usa eccezioni con inner exception e messaggio neutro, senza SQL interpolato. Aggiunti test per output dell'handler/null e preservazione dell'istanza/stack exception.

**Evidenza reale (Linux, 2026-10-17).**

```
$ rg -n 'throw\\s+ex\\s*;|catch\\s*\\{\\s*\\}|Console\\.Write(Line)?\\(ex(\\.Message)?\\)' Database PollingClass --glob '*.cs' | grep -v '//'
nessuna corrispondenza.
$ dotnet build Database/Database.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build Database/Database.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
exit 0; 0 errori.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet
111 passati, 2 ignorati, 0 falliti.
$ git diff --check
exit 0.
```

Windows/macOS non verificati localmente; la soluzione completa continua a fallire su Website MSB4249.

**Prossimo task:** `C7 — Config validata all'avvio`.

---

## 2026-10-17T11:30Z — C7 Config validata all'avvio — COMPLETATO

**Baseline:** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1, errore `MSB4249` sul Website WebForms; limite già noto e documentato. `Web.config` e `Web.config.example` contenevano provider `System.Data.EntityClient` per connection string SQL plain.

**Modifica:** `Database/AppConfig.cs` valida i provider legacy SQL Server e syntax/attributi server+database per ogni fonte di connection string; `PollingClass/QuartzSchedulerFactory.cs` valida connection string esplicite Quartz prima di creare lo scheduler. Corrette le due voci DB in `PollingWeb/Web.config` e `.example` a `System.Data.SqlClient`; nessun host default.

**Verifica reale:** `dotnet build Database/Database.csproj -f net8.0` e `-f net48` → exit 0; `dotnet build PollingClass/PollingClass.csproj -f net8.0` e `-f net48` → exit 0. `dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --nologo -v quiet` → superato, 114 passati/2 ignorati. `git diff --check` → exit 0. La build solution resta bloccata da MSB4249 (WebForms legacy).













---

## 2026-10-17T13:00Z — C10 SMTP — IN PROGRESS

**Baseline:** `MailSede.Invia` contiene credenziali placeholder nel codice, host soltanto da config, porta/TLS non configurabili, `UseDefaultCredentials=true` e nessun test SMTP. Evidenza attesa: configurazione validata host/porta/TLS/auth, invio su server SMTP fittizio e assenza credenziali hardcoded; build PollingClass net8.0/net48 e test unit/integration reali.

**Modifica parziale:** aggiunto `SmtpSettings` con host/porta/TLS/modalità credenziali e credenziali da `POLLING_SMTP_*`, validazione e destinatari; `MailSede` usa tali settings e dispone i client/message; aggiunta configurazione template Web.config, aggiornato SECURITY. Unit test validano configurazione/recipient; integration test avvia un sink SMTP TCP loopback sintetico e riceve 1 messaggio.

**Verifica iterazione corrente (Linux):** image SMTP4Dev scaricata (pull completato); `docker compose -f docker-compose.smtp.yml up -d` → servizio running; health verificato via log server in ascolto. Il test autenticato `MailSede_sends_authenticated_message_to_configured_fake_smtp_server` è stato eseguito con credenziali sintetiche passate solo in environment → 1/1 pass. Il log SMTP4Dev riporta autenticazione riuscita e messaggio ricevuto al destinatario sintetico. `dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet` e net48 → exit 0; unit 145/2 ignorati; `docker compose -f docker-compose.smtp.yml config --quiet` → exit 0; `git diff --check` → exit 0. Nessuna credenziale reale o di test versionata.

**Esito:** DoD C10 verificata su Linux con server SMTP4Dev containerizzato e autenticazione; Windows/macOS non verificati localmente. Il caso SMTP4Dev è abilitato e fallisce se endpoint manca, anziché saltare silenziosamente.

---

## 2026-10-17T13:45Z — C11 RDA_Esiti qualificazione — IN PROGRESS

**Baseline:** `sql/PollingDB/dbo/Stored Procedures/Esiti_Ordini_SAP.sql` eseguiva `INSERT INTO RDA_Esiti` non qualificata; DDL esiste in `sql/eProcurement/dbo/Tables/RDA_Esiti.sql`; l'app invoca la procedura usando `DatabaseInfo` applicativo `Polling`. Evidenza attesa: riprodurre l'errore cross-database e verificarne la correzione su SQL Server deployato.

**Modifica:** l'INSERT ora usa `eProcurement.dbo.RDA_Esiti`, coerente con la collocazione DDL e gli altri riferimenti della procedura. La verifica d'integrazione esegue `dbo.Esiti_Ordini_SAP` dal database `Polling` entro transazione rollbackable; nessuna fixture persistente aggiunta.

**Verifica reale:** deploy eseguito con `POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh --mode recreate --with-seed` → exit 0, schema Polling/eProcurement/Quartz applicato; `dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0; con `PATH=/opt/dotnet:$PATH`, `POLLING_DB_PASSWORD` di deploy e SMTP test env sintetico, test filtrato `Esiti_Ordini_SAP_executes_with_eProcurement_archive_target` → 1/1 pass. `./scripts/test-integration.sh` con `PATH=/opt/dotnet:$PATH` → fallisce per test non correlato `Polling_ha_gli_oggetti_attesi`: previsto 9 tabelle, trovato 10; il test C11 passa nella stessa suite.

**Verifica soluzione:** `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1, `MSB4249` (Website WebForms, limite noto).

**Esito C11:** chiuso `[x]` nel commit precedente; vedi §8 e `sql/eProcurement/README.md`.

---

## 2026-10-17T14:00Z — T2 Unit test logica pura — IN PROGRESS

**Baseline riprodotto:** le suite preesistenti coprivano soprattutto validazioni, filtro SQL e configurazione; Aggset e ParserFatture restano scarsamente coperte. Aggiunta configurazione Coverlet e due test sui wrapper Aggset che verificano il messaggio d'errore e la inner exception. Evitati refactoring alla logica di dominio.

**Verifica reale:** `PATH=/opt/dotnet:$PATH dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --collect:'XPlat Code Coverage' --nologo -v quiet` → 149 passati/2 ignorati. Report Cobertura: ValidaData 67,6%, Aggset 22,7%, ParserFatture 2,9%; T2 non completato per copertura insufficiente. Build `Database/Database.csproj` net8.0 e net48 exit 0; build `PollingClass/PollingClass.csproj` net8.0 e net48 exit 0 (net48: 5 warning); `git diff --check` exit 0. Soluzione non eseguita: blocco Website noto.

### 2026-10-17T15:00Z — T2 Unit test logica pura — IN PROGRESS (iterazione 45)

**Modifica:** aggiunti casi `ValidaData` per date ISO/invalidi e `IsNumeric` per precisione e input invalidi; aggiunto caso email vuota. Copertura misurata con Coverlet.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --collect:'XPlat Code Coverage' --nologo -v quiet` → 157 passati/2 ignorati. Build `Database/Database.csproj` e `PollingClass/PollingClass.csproj` con `-f net8.0` e `-f net48` → tutti exit 0; `git diff --check` → exit 0. Copertura `ValidaData` 67,6%, `Aggset` 22,7%, `ParserFatture` 2,9%: T2 non raggiunge DoD. `dotnet build PollingWeb.sln` non eseguito per Website WebForms noto; macOS/Windows non verificati localmente.

### 2026-10-17T16:00Z — T2 Unit test logica pura — IN PROGRESS (iterazione 46)

**Modifica:** aggiunti casi di data ISO (`yyyy-MM-dd`, datetime ISO) per verificare `ValidaData` sotto cultura esplicita; preservata la logica di dominio.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --collect:'XPlat Code Coverage' --nologo -v quiet` → 159 passati/2 ignorati. Coverage: `ValidaData` 67,6%, `IsNumeric` 51,0%, `ValidaMail` 78,6%, `Aggset` 22,7%, `ParserFatture` 2,9%. Build `Database/Database.csproj` e `PollingClass/PollingClass.csproj` con `-f net8.0` e `-f net48` → exit 0; `git diff --check` → exit 0. T2 non soddisfa ancora la copertura richiesta. Soluzione non eseguita; macOS/Windows non verificati localmente.

### 2026-10-17T17:00Z — T2 Unit test logica pura — IN PROGRESS (iterazione 47)

**Modifica:** aggiunti test per `IsNumeric.FEFormattato` con precisione specifica e default; nessuna modifica di produzione.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --collect:'XPlat Code Coverage' --nologo -v quiet` → 162 passati/2 ignorati. Coverage report: `ValidaData` 67,6%, `IsNumeric` 51,0%, `Aggset` 22,7%, `ParserFatture` 2,9%; T2 resta aperto. Build Database e PollingClass entrambi su `net8.0` e `net48` exit 0; `git diff --check` exit 0. Soluzione non eseguita, macOS/Windows non verificati localmente.

### 2026-10-17T18:00Z — T2 Unit test logica pura — BLOCKED (iterazione 48)

**Verifica reale:** `PATH=/opt/dotnet:$PATH dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --no-restore --collect:'XPlat Code Coverage' --nologo -v quiet` → 162 passati/2 ignorati (finale, dopo scarto dei due test esplorativi Aggset privi di database realistico). Coverage report: `ValidaData` 67,6%, `IsNumeric` 51,0%, `Aggset` 22,7%, `ParserFatture` 10,0%. Build `Database` e `PollingClass` net8.0/net48 exit 0 (net48 warnings); `git diff --check` exit 0. T2 incompleto oltre tre iterazioni; `[!]` secondo policy. Mitigazione: copertura DB alla suite d'integrazione e fixture SAP verificate; no refactor di dominio.

## 2026-10-17T19:00Z — T3 Integration test stored procedure e funzioni — IN PROGRESS

**Baseline riprodotto:** `SchemaContractTests` aveva casi per Datini/Datfin, StripHTML, Esiti_Ordini_SAP senza esito d'errore, ma mancavano test dimostrati per ogni procedura/flusso richiesto da T3. Evidenza attesa: chiamate a SQL Server 2022 con dati sintetici e rollback per tutte le scritture, output/effetti controllati.

**Modifiche precedenti all'iterazione 52:** introdotti test per entrambe le `StripHTML`, `Esiti_Ordini_SAP` con errore sintetico in rollback, `Flusso_RDA` e `Flusso_Ordini_SAP` con fixture RdA/item sintetiche, flussi per istituto senza match, rami insert/update di `ImportaBudget` e `AggiornaFornitori`, e invocazione di `AzzeraTargetSC00G100`. Le relative evidenze sono negli commit/registro precedenti.

### 2026-10-17T20:30Z — T3 Integration test stored procedure e funzioni — IN PROGRESS (iterazione 54)

**Modifica:** aggiunti test per `Flusso_Ordini_SAP` con fixture articolo/costo annuale e per `Flusso_Ordini_SAP_ist` con request/item NB dell'istituto sintetico; transazioni rollbackabili. Nessuna modifica alle routine/schema.

### 2026-10-18T00:45Z — T3 Integration test stored procedure e funzioni — IN PROGRESS (iterazione 62)

**Modifica:** aggiunto caso di conflitto `Esiti_Ordini_SAP`: ordine sintetico con esito positivo resta `Status=20` quando un altro ordine ha errore; tutto in transazione rollback.

**Evidenza reale:** `PATH=/opt/dotnet:$PATH dotnet build tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj --no-restore --nologo -v quiet` → exit 0. Filtro SQL mirato (6 casi) → 6/6 pass; `SchemaContractTests` → 28/28 pass. Full suite con SQL → 29 pass, 9 skip, 2 fail (conteggio schema e SMTP env); senza SMTP → 29 pass, 9 skip, 1 schema failure. `dotnet build PollingWeb.sln --nologo -v quiet` → exit 1 `MSB4249`. T3 resta `[~]`, DoD non completa.

---

## 2026-10-16T18:xxZ — X1.3c PollingClass multi-target — DONE

**Modifica.** `PollingClass.csproj` multi-target `net48;net8.0`. Per net8 vengono esclusi condizionalmente solo i metadata, attributi, metodi SOAP e base class ASMX che richiedono `System.Web.Services`; il target net48 li mantiene per la compatibilità legacy. Aggiunta la namespace import mancante nel test factory Quartz. Nessuna logica di dominio modificata. X3.3 resta incaricato della migrazione dei client SOAP.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingClass/PollingClass.csproj -f net8.0 --no-restore --nologo -v quiet
(exit 0; 0 errori, 5 avvisi)
$ dotnet build PollingClass/PollingClass.csproj -f net48 --no-restore --nologo -v quiet
(exit 0; 0 errori, 5 avvisi)
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 98. Ignorati: 2. Totale: 100.
$ git diff --check
(exit 0)
```

Misura: 4.178/4.298 righe non vuote compilabili per net8 (97,2%, sopra la soglia 95%). I 120 conteggi non compilabili sono i soli blocchi di attributi/ereditarietà e attributi WebMethod relativi agli endpoint ASMX in `Import_Budget.cs`, `ImportFatture.cs`, `LeggiFornitore.cs`, `Ordini_Sap.cs`, conservati integralmente per net48. Windows/macOS non verificati localmente; la soluzione completa WebForms non compilata.

**Prossimo task:** `X1.1 — Layout soluzione + Directory.Build.props`.

---

## 2026-10-16T16:xxZ — X1.3b4 Separazione Ordini_Scheduler/Quartz — DONE

**Modifica.** `Ordini_Scheduler` delega FirstStart e i due polling (invio/esito) a `IOrdiniSchedulingGateway`. Aggiunti `PollingClass/OrdiniQuartzGateway.cs` e `PollingClass/JobOrdini.cs` (i due job restano distinti); `Ordini_Sap.cs` non contiene più riferimenti Quartz. L'adapter conserva identità di job/trigger, routing Invio/Esito, `tipoExport` solo per l'invio, cron feriale, override, JobDataMap e trigger immediate/interval con priorità legacy. Separata policy validazione cron, interval e routing.

**Evidenza reale (Linux, 2026-10-16).**

```
$ dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet
Compilazione completata. Avvisi: 5. Errori: 0.
$ dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet
Superato! Non superati: 0. Superati: 90. Ignorati: 1. Totale: 91.
$ grep -nE '\\b(Quartz|IJob|StdSchedulerFactory|JobBuilder|TriggerBuilder|IScheduler)\\b' PollingClass/Ordini_Sap.cs
(nessuna corrispondenza)
$ git diff --check
(exit 0)
```

**Limiti.** `PollingClass` resta net48; verificata solo la build Linux con reference assemblies. Windows/macOS non verificati localmente; la soluzione WebForms non è stata compilata. I test unit coprono policy cron/interval e routing dei due tipi job, non l'esecuzione contro Quartz reale.

**Prossimo task:** `X1.3b5 — SQL01_Scheduler`.

---
