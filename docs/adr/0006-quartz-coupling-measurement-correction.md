# ADR 0006 — Correzione della misura di accoppiamento a Quartz e scomposizione di X1.3b

- **Stato:** accettata
- **Data:** 2026-10-16
- **Corregge:** [ADR 0005](0005-multitarget-perimeter-and-refactoring-safety-net.md) §1.1 (misura) e §2.2 (scomposizione)
- **Contesto del piano:** task `X1.3b`, `X1.3c`

---

## 1. Che cosa ha sbagliato l'ADR 0005, e perché

L'ADR 0005 dichiarava:

> «Il 55% dell'assembly non è compilabile per net8 — 2 905 righe su 5 246.»

**La misura era troppo grossolana e sovrastimava il problema.** Il metodo era: *contare le righe
dei file che citano Quartz o `System.Web.Services`*. Ma un file che contiene **una** chiamata a
Quartz può avere 700 righe di logica di dominio perfettamente portabile: contare il file intero
attribuisce a Quartz anche ciò che non lo riguarda.

Misura rifatta sui **simboli**, non sui file:

| Classe accoppiata a Quartz | File | Righe |
|---|---:|---:|
| `ExportSQL01` | `ExportSQL01.cs` | 403 |
| `Fornitori_Scheduler` | `Prog_Quartz.cs` | 191 |
| `ImportFatture_Scheduler` | `ImportFatture.cs` | 191 |
| `Ordini_Scheduler` | `Ordini_Sap.cs` | 180 |
| `ImportBudget_Scheduler` | `Import_Budget.cs` | 146 |
| `SQL01_Scheduler` | `ExportSQL01.cs` | 142 |
| `JobFornitori`, `JobImportFatture`, `JobOrdini_SAP_Invio`, `JobOrdini_SAP_Esito`, `JobBudget`, `JobSQL01` | vari | 177 |
| **totale classi coinvolte** | | **1 430** |

E, dentro quelle classi, quanto è **davvero** API Quartz? Misurato su `Fornitori_Scheduler`:
**29 righe su 191 (15%)**. Il resto è lettura di `TaskService`, calcolo dell'espressione cron dai
giorni di schedulazione, gestione errori e logging: tutto platform-neutral.

**Conclusione corretta:** la superficie da confinare non è il 55% dell'assembly. È circa il
**15% di 1 430 righe ≈ 215 righe**, più le 177 righe delle sei classi `IJob`. Il residuo
net48-only atteso dopo `X1.3b` è dell'ordine di **400 righe su 5 236 (≈ 8%)**.

## 2. Conseguenza sulla soglia anti-svuotamento

`X1.3c` chiedeva `righe(net8) / righe(totali) ≥ 95%`. Con il residuo reale di ~400 righe il
rapporto atteso è **≈ 92%**: la soglia del 95% era fissata su una stima sbagliata.

| Momento | Residuo net48-only | Rapporto atteso |
|---|---|---|
| dopo `X1.3b`, con Quartz 2.x | ~400 righe (6 classi `IJob` + adattatori) | **≥ 90%** |
| dopo `X2.2`, con Quartz 3.x (`netstandard2.0`) | le classi `IJob` tornano disponibili su net8 | **≥ 95%** |

Soglia rivista: **≥ 90% per `X1.3c`**, **≥ 95% dopo `X2.2`**. La regola anti-svuotamento resta:
il rapporto va riportato nel registro, con l'elenco esplicito delle esclusioni. Cambia il numero,
non il principio.

## 3. `X1.3b` scomposto per scheduler

Il lavoro vero di `X1.3b` è **separare, in ogni classe `*_Scheduler`, il calcolo della
schedulazione dall'invocazione di Quartz**: il primo è platform-neutral e diventa
**testabile su net8**; la seconda finisce in un adattatore net48-only.

Sono 5 classi da ~190 righe, ognuna con struttura omogenea (`FirstStart` che legge `TaskService`
e compone un'espressione cron; `Polling_Interval` che pianifica). Farlo in un colpo solo
significa 950 righe di ristrutturazione senza un pattern di riferimento già validato.

`X1.3b` è quindi scomposto:

| Task | Classe | Nota |
|---|---|---|
| **`X1.3b1`** | `Fornitori_Scheduler` (`Prog_Quartz.cs`) | **implementazione di riferimento**: definisce l'interfaccia e il pattern per gli altri quattro |
| `X1.3b2` | `ImportFatture_Scheduler` | |
| `X1.3b3` | `ImportBudget_Scheduler` | |
| `X1.3b4` | `Ordini_Scheduler` | due trigger (invio ed esito) |
| `X1.3b5` | `SQL01_Scheduler` | |

Ogni task si chiude con: build net48 a 0 errori, test verdi, e il file di dominio che **non cita
più simboli Quartz**.

## 4. Alternative considerate

| Alternativa | Perché non scelta |
|---|---|
| Mantenere la stima del 55% e la soglia del 95% | Sarebbe una decisione presa su un numero falso: la soglia del 95% avrebbe bloccato `X1.3c` per un motivo inesistente |
| Abilitare subito `net48;net8.0` escludendo le classi accoppiate, senza separazione | Copertura net8 al 73%: sotto soglia, e `X1.3b` resterebbe comunque da fare |
| Estrarre tutto in colpo solo (950 righe) | È la modalità di lavoro che l'ADR 0005 §1.3 ha già dichiarato rischiosa: con i test si riduce il rischio, non lo si annulla |
| Non separare e portare solo il futuro `PollingCore` | Duplicherebbe la logica di schedulazione invece di spostarla |

## 5. Lezione registrata

Una misura fatta con l'euristica sbagliata ha prodotto una revisione del piano **più pessimistica
del necessario** (ADR 0005 §2.2). Il costo è stato una scomposizione eccessiva; il beneficio è
che la misura rifatta è più precisa e ora distingue *file coinvolti* da *righe accoppiate*.

Regola operativa che ne deriva: **misurare i simboli, non i contenitori**. Per un'analisi di
accoppiamento, contare i file che citano una dipendenza è un indicatore di *dove guardare*, non
una misura di *quanto costa*.
