# Campioni tracciati SAP

Formati ricostruiti dal **codice di lettura**, non da documentazione SAP. Se il tracciato
reale differisce, il codice di parsing va corretto: questi file sono il riferimento
condiviso per i test.

---

## `oda_esiti_input.sample`

Esito dell'invio ordini a SAP. Letto da `PollingClass/Ordini_Sap.cs` (`StreamReader` +
`Substring`) a **larghezza fissa**, senza delimitatore.

| Offset | Lunghezza | Campo | Note |
|--------|-----------|-------|------|
| 0 | 3 | `Mandante` | es. `T47` |
| 3 | 4 | `Societa` | codice sintetico |
| 7 | 10 | `Num_Ordine` | zero-padded |
| 17 | 3 | `Posizione` | |
| 20 | 5 | `Cod_Errore` | tutti zeri = nessun errore |
| 25 | resto | `Desc_Errore` | testo libero |

```
<RECORD-40-CHARS>
│  │   │         │  │
│  │   │         │  └─ Cod_Errore
│  │   │         └──── Posizione
│  │   └────────────── Num_Ordine
│  └────────────────── Societa
└───────────────────── Mandante
```

Alcune righe del campione hanno un codice errore non-zero per esercitare il ramo d'errore.
Il file reale era padded con spazi a larghezza fissa: il parser non lo richiede.

---

## `rda_input.sample`

Input del flusso RDA (richieste di acquisto).

Il file reale osservato nel repository storico era **vuoto** (3 caratteri di nuova riga,
nessun contenuto). Questo campione riproduce esattamente quel contenuto: è un caso limite
deliberato, non un file dimenticato.

Serve a verificare che il lettore **tolleri l'input vuoto senza eccezioni** — comportamento
oggi non garantito e oggetto del task `C9` (robustezza del parsing dei tracciati SAP).

Il tracciato **popolato** non è documentato qui: va ricavato da `Flusso_RDA` / `Flusso_RDA_ist`
e consolidato insieme al contratto SAP in un ADR (`docs/adr/`). Inventare righe plausibili
ma non verificate sarebbe peggio che dichiarare la lacuna.

---

## `SC00G100.sample.txt`

Estratto dell'anagrafica fornitori, letto da `PollingClass/LeggiFornitore.cs`, che accetta
**due dialetti** sulla stessa riga:

1. `|` (pipe) — estratti più recenti. Il parser accetta sia la riga con `|` iniziale
   (9 campi) sia senza (8 campi: viene anteposto un `|`).
2. `\xB3` (`³`, code page 1252) — estratti storici. Il parser antepone il carattere e
   rimuove un carattere in testa al campo 1.

In entrambi i casi la riga si divide in **9 campi**, di cui il primo e l'ultimo vuoti:

| Indice | Mappato su | Note |
|--------|-----------|------|
| 0 | — | vuoto (delimitatore iniziale) |
| 1 | `Cfor` | codice fornitore, **deve superare `IsNumeric`**, viene `.Trim()`ato |
| 2 | `Ragione_Sociale` | |
| 3 | `Indirizzo` | |
| 4 | `Prov` | sigla provincia |
| 5 | `Cap` | |
| 6 | `Citta` | |
| 7 | `Piva` **oppure** `Codfis` | se sono 11 caratteri → `Piva`, altrimenti → `Codfis`; l'altro campo resta vuoto |
| 8 | — | vuoto (delimitatore finale) |

⚠️ L'`Istituto` **non** viene letto dal file: è preso dalla configurazione
(`codice_istituto`) e scritto in `dr["Istituto"]`.

Il campione usa record costruiti per test sintetici, inclusa una riga che esercita il ramo alternativo del parser.

**Non testato dai campioni:** il dialetto `³` richiede un file in code page 1252, che non si
può rappresentare in un `.txt` UTF-8. Va aggiunto come fixture binaria quando si scriverà il
test `C9`; fino ad allora la sua copertura è dichiarata mancante, non assunta.
