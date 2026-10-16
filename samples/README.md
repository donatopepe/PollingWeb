# Campioni sintetici (samples/)

> **Tutti i dati in questa directory sono inventati.** Nessun dato reale di clienti,
> fornitori o dipendenti è presente. Gli identificativi fiscali sono segnaposto evidenti
> (`00000000001`…), le ragioni sociali sono fittizie, gli importi sono arbitrari.

## Perché esistono

I file reali che il sistema elabora — tracciati SAP, anagrafiche fornitori, estrazioni di
budget, dump di database — sono di titolarità di terzi e **non possono essere pubblicati**.
Questi campioni servono a:

- smoke test dei parser (`ParserFatture`, `ImportaXML`, `LeggiFornitore`, `Ordini_Sap`);
- test di integrazione e regressione (fase `T` del piano) senza dipendere da dati reali;
- documentare con un esempio concreto il formato atteso da SAP.

## Contenuto

| Percorso | Formato | Usato da |
|----------|---------|----------|
| `sap/oda_esiti_input.sample` | testo a larghezza fissa | `PollingClass/Ordini_Sap.cs` (`Esegui`) |
| `sap/rda_input.sample` | testo, una richiesta per riga | flusso RDA (`Flusso_RDA*`) |
| `sap/SC00G100.sample.txt` | pipe-delimited, 9 campi | `PollingClass/LeggiFornitore.cs` |
| `budget/BDG_sample.xlsx` | XLSX, foglio `BDG`, 7 colonne | `PollingClass/ImportaXML.cs` |
| `budget/README.md` | — | descrizione del foglio |
| `seed/seed.sql` | T-SQL | dati minimi per tabelle `Dim*`, `TaskService`, `TargetSC00G100` |
| `tools/make-budget-sample.py` | Python 3 (solo stdlib) | rigenera `budget/BDG_sample.xlsx` |

## Rigenerare il campione XLSX

```bash
python3 samples/tools/make-budget-sample.py
# scritto samples/budget/BDG_sample.xlsx (2171 byte), foglio 'BDG', 4 righe dati
```

Il file è deterministico: stessi input → stessi bytes. Se il parser cambia, il campione
resta confrontabile in un diff binario.

## Regole per chi aggiunge campioni

1. **Mai** copiare un file di produzione, nemmeno "ripulito a mano".
2. **Mai** usare nomi, indirizzi, P.IVA o codici fiscali plausibili ma reali.
3. Preferire un generatore (script) a un binario opaco quando il formato è complesso.
4. Descrivere nel `README` della sottocartella il formato e quale codice lo consuma.
5. Se il campione ha un caso limite (file vuoto, riga troncata, encoding sbagliato),
   dichiararlo esplicitamente: è materiale di test, non rumore.

## Materiale volutamente escluso

| File rimosso (task `T0.3`) | Motivo |
|---|---|
| `script_eprocurement_donato.sql`, `script_polling_donato.sql` | dump con 15.732 `INSERT` di dati reali |
| `SC00G100_inizio.TXT` | anagrafica fornitori reale con P.IVA e codici fiscali |
| `BDG_MCC.xlsx` | estrazione di budget reale |
| `_047_1047_oda_input.#00`, `_063_1063_rda_input.#00` | tracciati SAP reali |
| `*/Importa log di schema/*.log` (due database project) | log con nomi di macchine e utenti di sviluppo |
| `script_polling.sql` | snapshot di schema di 3,3 MB; oggetti utili estratti in `sql/PollingDB/dbo/Tables/` |
