# ADR 0005 — Perimetro del multi-target e rete di sicurezza per il refactoring

- **Stato:** accettata
- **Data:** 2026-10-16
- **Contesto del piano:** task `X1.3`, `X1.3b` (nuovo), `X1.3c` (nuovo), `X2.2`, `T1`
- **Collegata a:** [ADR 0004](0004-build-layout-and-multitargeting.md) (layout di build)

---

## 1. Problema

`X1.3` chiede che `PollingClass` diventi multi-target `net48;net8.0` con evidenza «build per
entrambi i TFM → 0 errori». Misurando il codice prima di eseguire, l'obiettivo si rivela
raggiungibile **solo escludendo la maggior parte del dominio**.

### 1.1 Misura

Classificazione dei 21 file di `PollingClass` per dipendenza da API che non esistono su net8:

| File | Righe | Quartz 2.x | `System.Web.Services` |
|------|------:|-----------:|----------------------:|
| `Ordini_Sap.cs` | 721 | 34 | 8 |
| `ImportFatture.cs` | 621 | 25 | 4 |
| `Import_Budget.cs` | 478 | 21 | 3 |
| `ExportSQL01.cs` | 449 | 20 | — |
| `LeggiFornitore.cs` | 381 | — | 4 |
| `Prog_Quartz.cs` | 255 | 25 | — |
| **totale non compilabile per net8** | **2 905** | | |
| **totale `PollingClass`** | **5 246** | | |

**Il 55% dell'assembly non è compilabile per net8.** Non è una percentuale di dettaglio: quei
file contengono **tutti i flussi di dominio** — invio ordini a SAP, import budget, import
fatture, lettura fornitori, export. Il restante 45% sono parser, wrapper di stored procedure,
logging e notifiche.

### 1.2 Perché non è un problema di configurazione

- **Quartz 2.1.2** dichiara il solo target `net40`. Non è un pacchetto «da verificare»: le sue
  API non esistono su .NET 8. La sostituzione è `X2.2` (Quartz 3.x + job store ADO).
- **`System.Web.Services.WebService`** e `[WebMethod]` sono ASMX, **non supportati** su .NET
  (fatto `P3`). I quattro wrapper vanno separati dalla logica.

### 1.3 Il refactoring necessario non ha rete di sicurezza

Separare i quattro wrapper ASMX dalla logica significa ristrutturare **2 201 righe di codice
stateful** (`codice_istituto`, `IdProcesso`, `OneTime` sono campi di istanza condivisi fra
superficie web e logica) in classi di servizio, con i wrapper ridotti a deleganti.

Oggi **non esiste alcun test** in questo repository: l'infrastruttura di test è `T1`, che viene
dopo. Con l'unica evidenza disponibile — «compila» — una regressione di comportamento non
sarebbe rilevabile. Un refactoring di quella dimensione senza test, eseguito in modo
incrementale e automatico, è il modo più affidabile per rompere un sistema che funziona.

## 2. Decisione

### 2.1 `X1.3` consegna la parte verificabile e a rischio nullo

`X1.3` diventa: `PollingClass` **SDK-style su net48**, con

- rimozione di `LinqToExcel` e `Remotion` (LGPL, Windows-only);
- rimozione del riferimento `WindowsBase` (non usato);
- passaggio a `Microsoft.Data.SqlClient`;
- build riproducibile su Linux (oggi impossibile: `MSB3644`).

Evidenza: `dotnet build PollingClass/PollingClass.csproj -f net48` → 0 errori;
`git grep -i "remotion\|linqtoexcel"` = 0.

Non è un ripiego: senza questo, `PollingClass` **non si compila affatto** su un sistema che non
sia Windows con il Developer Pack, quindi nessun altro task può essere verificato.

### 2.2 Il multi-target si scompone in due task, ciascuno con il suo prerequisito

| Task | Contenuto | Prerequisito | Perché |
|------|-----------|--------------|--------|
| **`X1.3b`** | estrazione della logica dei 4 ASMX in classi di servizio platform-neutral; `Prog_Quartz` dietro interfaccia | **`T1`** (infrastruttura di test) | 2 201 righe stateful senza test: serve una rete di sicurezza *prima*, non dopo |
| **`X1.3c`** | `TargetFrameworks=net48;net8.0` su `PollingClass` | `X1.3b` **e** `X2.2` (Quartz 3.x) | finché Quartz 2.x è una dipendenza, il 55% del codice non può stare nel target net8 |

L'ordine risultante: `X1.3` → `T1` → `X1.3b` → `X2.2` → `X1.3c`.

### 2.3 Regola anti-svuotamento (vale da `X1.3c` in poi)

Un task di multi-target **non può** essere chiuso escludendo codice dal target senza dichiararlo.
`X1.3c` deve riportare nel registro:

- il numero di righe compilate per **ciascun** target;
- il rapporto `righe(net8) / righe(totali)`, con **soglia ≥ 95%**;
- l'elenco esplicito di eventuali file esclusi, con il task che li porterà a bordo.

Senza questa regola, «multi-target» sarebbe soddisfacibile mettendo i file difficili in una
lista di esclusione, e l'assembly net8 potrebbe non contenere il motore: l'evidenza «compila»
passerebbe e l'affermazione «cross-platform» sarebbe falsa.

## 3. Conseguenze

**Positive**

- Ogni task ha un'evidenza che può passare e che **misura** qualcosa (righe, dipendenze).
- Il refactoring più rischioso (`X1.3b`) avviene quando esistono i test.
- `PollingClass` diventa compilabile su Linux già con `X1.3`: sblocca la verificabilità di tutti
  i task successivi.

**Negative / da gestire**

- Il target net8 di `PollingClass` arriva più tardi di quanto il piano prevedesse: la fase `X2`
  (worker) e `X3` (API) sono comunque bloccate da `X2.2`, quindi il ritardo non è sulla
  critical path del risultato finale.
- Tre task al posto di uno: più passaggi nel registro, a fronte di evidenze granulari.

## 4. Alternative considerate

| Alternativa | Perché non scelta |
|---|---|
| Eseguire `X1.3` come scritto, escludendo i 6 file dal target net8 | Passerebbe l'evidenza «compila» con il 55% del dominio assente: un'affermazione falsa sulla portabilità, esattamente ciò che la regola anti-svuotamento vieta |
| Aggiornare Quartz a 3.x dentro `X1.3` | Sarebbe `X2.2` dentro un altro task: cambia le API di scheduling, richiede lo schema `QRTZ_` verificato e un ADR proprio. Mescolarlo a `X1.3` renderebbe un fallimento non attribuibile |
| Estrarre i 4 ASMX adesso, senza test | 2 201 righe stateful senza rete di sicurezza: un errore silenzioso nei flussi (ordini, fatture, budget) non verrebbe rilevato da «compila» |
| Rinunciare al multi-target di `PollingClass` e portare solo il futuro `PollingCore` | Duplicherebbe il codice invece di spostarlo, e lascerebbe la logica esistente non verificabile |

## 5. Verifica

```
# X1.3 — build riproducibile su Linux, target legacy
dotnet build PollingClass/PollingClass.csproj -f net48        -> 0 errori
git grep -i "remotion\|linqtoexcel"                           -> 0 risultati

# X1.3c — perimetro dichiarato e non svuotato
#   righe compilate per net8 / righe totali >= 95%  (da riportare nel registro)
```
