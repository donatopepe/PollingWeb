# eProcurement — schema NON versionato

Questo database è referenziato dal codice e dalle stored procedure, ma **il suo schema non è
presente in questo repository**. La cartella è vuota per rendere il problema visibile invece
che silenzioso.

## Cosa è referenziato

Un'analisi dei riferimenti espliciti `eProcurement.dbo.*` in `sql/` e in `PollingClass/`
trova **19 oggetti**:

```
DimBudget                DimBudgetTemp            DimCentriDireSp
DimFornitori             DimSottocatSAP           JoinCatSottocat
Log_ODA_SAP              LogTime_Ciclo            RdA
RdAArt                   RdAArt_Costo_Per_Anno    RdE
Tab_SC00S100_1Ord        Tab_SC00S100_1Ord_R      Tab_SC00S100_2Fatt
Tab_SC00S100_2Fatt_R     RDA_Esiti_LastOutput_OnLine
StripHTML                Tbl_Export_SAP_OnLine
```

Solo **3** risultano definiti nel repository — `StripHTML`, `RDA_Esiti_LastOutput_OnLine`,
`Tbl_Export_SAP_OnLine` — e i primi due con collocazione ambigua: sono creati nel database
**applicativo**, mentre le procedure li referenziano come `eProcurement.dbo.*`.

**Conseguenza:** un deploy da questo repository produce un database `eProcurement` vuoto. Le
stored procedure vengono create (SQL Server non valida gli oggetti referenziati alla
creazione: name resolution differita), quindi il deploy **riesce**, ma falliscono alla prima
esecuzione. Il sistema non è distribuibile nella sua interezza senza questo schema.

Verificato il 2026-10-16 dopo un deploy completo:

```
eProcurement: tabelle=0
```

## Perché manca e come si recupera

Lo schema esisteva in uno snapshot del database conservato nel record di audit offline
(`script_eprocurement_donato.sql`), rimosso dal repository in `T0.3` perché conteneva **15.732
istruzioni `INSERT` di dati reali** — anagrafiche fornitori, richieste di acquisto, P.IVA,
indirizzi email aziendali.

Il recupero deve estrarre **solo la DDL** (`CREATE TABLE`, `CREATE VIEW`, indici, vincoli),
scartando integralmente ogni `INSERT`, e verificare il risultato contro i pattern del record di
audit prima di introdurlo nel repository. È il task **`T0.12`**; la strategia è descritta in
[`docs/adr/0001-sql-schema-source-of-truth.md`](../../docs/adr/0001-sql-schema-source-of-truth.md) §4.

## Cosa NON fare

- **Non** copiare lo snapshot nel repository, nemmeno "ripulito a mano".
- **Non** ricostruire lo schema a intuito leggendo i riferimenti nel codice: le colonne sono
  usate in modo implicito e i tipi ricostruiti sarebbero congetturali.
- **Non** dichiarare il progetto distribuibile finché questo schema non è presente e
  verificato.
