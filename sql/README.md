# sql/

Script SQL del progetto.

## Stato attuale (dopo `T0.6`)

| File | Contenuto |
|------|-----------|
| `quartz-schema.sql` | schema del job store ADO di Quartz.NET: 11 tabelle `QRTZ_*` |

## Nota importante — la struttura è provvisoria

Lo schema applicativo vive ancora **duplicato** in due database project SSDT:

- `DatabaseSQL/` — contiene i file `dbo/**/*.sql` ma il `.sqlproj` **non include alcun
  `<Build Include>`**: i file non vengono compilati, sono di fatto orfani;
- `PollingDB/` — è il progetto che compila davvero, con `<Build Include>` per tabelle,
  viste, stored procedure, funzioni e script di security.

Inoltre il nome della directory e del progetto contiene l'abbreviazione dell'istituto per
cui il software è stato sviluppato: va rimosso (task `T0.11`).

Il task **`T0.7`** consolida tutto in questa cartella come **unica fonte di verità**:

```
sql/
  PollingDB/          schema del database applicativo
  eProcurement/        schema del database di acquisto
  Quartz/              schema del job store (quartz-schema.sql)
  Security/            login, utenti e permessi a privilegi minimi
  deploy/              ordine di applicazione, idempotenza, migrazioni
```

con un ADR che motiva la scelta (`docs/adr/0001-sql-schema-source-of-truth.md`).

Fino ad allora **non considerare coerente** quanto presente in `DatabaseSQL/` e
`PollingDB/`: la divergenza ha già causato la perdita quasi avvenuta di due tabelle
(vedi `docs/LOOP_LOG.md`, task `T0.3`).
