# Campione import budget

`BDG_sample.xlsx` è generato da `../tools/make-budget-sample.py`. Non modificarlo a mano:
rigenerarlo, così il contenuto resta riproducibile e verificabile.

## Struttura attesa dal parser

`PollingClass/ImportaXML.ReadExcelFileDOM` non cerca il foglio per posizione: lo cerca
**per nome** (`BDG`) e legge le colonne **per indice di cella** nella riga.

| Colonna | Proprietà `Budget` | Contenuto |
|---------|--------------------|-----------|
| A (1) | `IST` | codice istituto |
| B (2) | `CDR` | centro di responsabilità |
| C (3) | `VOCE` | voce di budget |
| D (4) | `BDGRILASCI` | budget rilasciato |
| E (5) | `BDGRESIDUO` | budget residuo |
| F (6) | `CODFONDO` | codice fondo |
| G (7) | `DATARIF` | data di riferimento |

Vincoli impliciti che il campione rispetta:

- il foglio deve chiamarsi esattamente `BDG`, altrimenti `FirstOrDefault()` restituisce
  `null` e il codice va in `NullReferenceException` (difetto da correggere in `C9`);
- le celle devono essere **shared string** (`t="s"`): il parser risolve l'indice nella
  `SharedStringTable`; celle inline o numeriche non gestite producono valori errati;
- la riga di intestazione del campione è dati come le altre — il parser non salta righe.

## Rigenerare

```bash
python3 samples/tools/make-budget-sample.py
# scritto samples/budget/BDG_sample.xlsx (2171 byte), foglio 'BDG', 4 righe dati
```

## Verifica

Il campione è stato aperto con la stessa libreria del codice applicativo
(`DocumentFormat.OpenXml 2.7.1`) replicando la logica di `ReadExcelFileDOM`:

```
[IST][CDR][VOCE][BDGRILASCI][BDGRESIDUO][CODFONDO][DATARIF]
[1][DEMO01][V0001][1000.00][1000.00][FONDO-A][2024-01-01]
OK righe=5 celle=35 sharedStringsIrrisolte=0
```

`sharedStringsIrrisolte=0` è il controllo che conta: se la `SharedStringTable` fosse assente
o malformata, il parser produrrebbe stringhe vuote invece dei valori.
