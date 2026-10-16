# Tabelle mancanti — richiesta all'ambiente di origine

> **Stato: BLOCCATO.** Task `T0.13`. La DDL di queste 4 tabelle **non esiste in alcuna fonte
> disponibile**: non nel repository, non nella sua storia archiviata, non nello snapshot del
> database `eProcurement` recuperato in `T0.12`. Va richiesta all'ambiente di origine.
>
> Questo documento **non ricostruisce** le tabelle. Riporta con precisione *cosa serve*, per
> rendere la richiesta attuabile e verificabile. Ricostruire tipi e vincoli dai soli
> riferimenti nel codice sarebbe congetturale: produrrebbe uno schema che "sembra" corretto e
> fallisce sui dati reali.

## 1. Ricerca esaustiva eseguita (nessuna fonte trovata)

| Fonte interrogata | Esito |
|---|---|
| `sql/PollingDB/`, `sql/eProcurement/` | assenti |
| Snapshot `eProcurement` (17 oggetti, `T0.12`) | assenti: contiene 15 tabelle, nessuna è `Tab_SC00S100_*` |
| Storia Git archiviata (9 commit, tutti i blob) | i nomi compaiono **solo** in `PollingClass/ParserFatture.cs` e `PollingClass/Check_RdA.cs`. Nessuno script, nessuna DDL |
| Codice applicativo | riferimenti presenti, ma nessuna definizione |

```
$ (su tutti i blob di tutti i commit della storia archiviata)
  blob che contengono 'Tab_SC00S100_1Ord|Tab_SC00S100_2Fatt'
  -> 8 blob, tutti versioni dei due file .cs. Nessuno script SQL.
```

## 2. Cosa serve

Per ciascuna tabella: **DDL completa** (`CREATE TABLE` con tipi, nullabilità, collation),
chiavi primarie, chiavi esterne, indici e vincoli `DEFAULT`. Il formato preferito è uno script
SSDT, come per le altre tabelle in `sql/eProcurement/dbo/Tables/`.

### `Tab_SC00S100_1Ord` — testata riga d'ordine

Usata da `ParserFatture.cs`. Operazioni: `SELECT`, `INSERT INTO`. Mai cancellata.

| Operazione | Colonne usate dal codice | Riga |
|---|---|---|
| `SELECT` | `NOrdine` (nella `WHERE`) | 277, 560 |
| `INSERT INTO` | `Tab_Name`, `Tab_Prog`, `Istituto`, `Data_Rif`, `NOrdine`, `Tipo`, `CodFor`, `RagSoc`, `GrA`, `Data_OrdAcq` | 299 |

> Il `SELECT` è un test di esistenza: `SELECT [NOrdine] FROM ... WHERE [NOrdine] = <n>`.
> Quindi `NOrdine` deve essere selezionabile e filtrabile; la tabella riceve una riga per
> ordine se non è già presente.

### `Tab_SC00S100_1Ord_R` — righe dell'ordine

| Operazione | Colonne usate dal codice | Riga |
|---|---|---|
| `SELECT` | `NOrdine` (filtro), `Perc_DaFatt` | 345 |
| `INSERT INTO` | `NOrdine`, `NRiga`, `Materiale`, `Testo_Breve`, `Gr_Merci`, `Qta`, `UMO`, `Prz_Netto` | 383 |
| `UPDATE … SET` | `Qta_DaFatt`, `UMO_DaFatt`, `Prz_Netto_DaFatt`, `Perc_DaFatt`; filtro su `NOrdine`, `NRiga` | 431 |

> La `UPDATE` scrive le colonne `*_DaFatt` che non compaiono mai in `INSERT`: esistono nella
> tabella ma non sono valorizzate al momento dell'inserimento. Devono essere **nullable**.

### `Tab_SC00S100_2Fatt` — testata fattura

| Operazione | Colonne usate dal codice | Riga |
|---|---|---|
| `SELECT` | `NOrdine`, `NFatt` (filtro) | 458 |
| `INSERT INTO` | `NOrdine`, `NFatt`, `Data_Fatt` | 470 |

### `Tab_SC00S100_2Fatt_R` — righe fattura

| Operazione | Colonne usate dal codice | Riga |
|---|---|---|
| `SELECT` | `NOrdine`, `NFatt`, `NPos` (filtro) | 492 |
| `INSERT INTO` | `NOrdine`, `NFatt`, `NPos`, `Qta_Tot`, `UMO`, `Prz_Tot` | 506 |

## 3. Come è stata prodotta questa specifica

Estratta meccanicamente dal codice con uno script, non a mano. Due errori dell'estrattore,
corretti prima di pubblicare l'elenco:

1. il nome della tabella compare spesso nel frammento `FROM` e non nel `SELECT`: cercare la
   parola chiave solo sulla riga della tabella **faceva sparire tutte le `SELECT`**;
2. una finestra di righe troppo larga catturava l'istruzione successiva, attribuendo le colonne
   di `Tab_SC00S100_1Ord_R` a `Tab_SC00S100_1Ord`.

Il secondo difetto è insidioso: `Tab_SC00S100_1Ord` è **prefisso** di `Tab_SC00S100_1Ord_R`,
quindi la ricerca per elenco attribuiva le istruzioni della tabella `_R` a quella senza
suffisso. L'attribuzione ora procede **dal nome più lungo al più corto**.

Gli elenchi sopra sono quindi colonne **usate dal codice**, non la definizione completa della
tabella: la tabella reale può avere altre colonne. Serve la DDL.

## 4. Cosa NON fare, e perché

| Scorciatoia | Conseguenza |
|---|---|
| Ricostruire le tabelle con tutti i campi `nvarchar(max)` | Il codice converte i valori (`getdouble`, `getInt`, `TextToDate`): tipi sbagliati producono errori di conversione o troncamenti silenziosi sui dati reali |
| Ricostruire con i soli campi elencati qui | Le colonne della tabella reale sono di più: `UPDATE` e `SELECT` dell'applicazione non sono l'unico consumatore (le procedure `Flusso_*` e SAP possono usarne altre) |
| Creare le tabelle nel database applicativo invece che in `eProcurement` | Il codice le qualifica esplicitamente come `[eProcurement].[dbo].[...]`: finirebbero nel posto sbagliato |
| Chiudere il task senza la DDL | Il repository resterebbe non distribuibile, ma con l'illusione del contrario |

## 5. Impatto finché il task è bloccato

- Il deploy `scripts/db-deploy.sh` funziona e crea **19** tabelle in `eProcurement` (15 + le 4
  mancanti diventerebbero 19): oggi ne crea 15.
- `ParserFatture` — il flusso di importazione fatture — **non può funzionare**: tutte e quattro
  le tabelle sono coinvolte in ogni esecuzione.
- Il criterio di accettazione **AC3** («il motore esegue un job reale») non è raggiungibile per
  il processo fatture finché mancano.

## 6. Richiesta all'ambiente di origine

Da chiedere, in ordine di preferenza:

1. **script di creazione** delle 4 tabelle (SSDT / SSMS "Script Table as CREATE"), completo di
   indici e vincoli;
2. in mancanza, l'**output di** `sp_help` o di
   `SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME LIKE 'Tab_SC00S100%'`;
3. in ultima istanza, uno **script di export dello schema** del database `eProcurement`
   completo, dal quale estrarre la sola DDL con la procedura di `T0.12` (che scarta i dati
   **prima** di scrivere qualunque file).

Nel caso (3) vale la stessa regola di `T0.12`: **solo DDL**, nessun `INSERT`, verifica contro i
pattern del record di audit offline prima di introdurre il risultato nel repository.
