# tests/ — infrastruttura di test

Task `T1`. Due progetti, entrambi **target `net8.0`**.

## Perché `net8.0` e non `net48`

I test devono essere **eseguiti**, non solo compilati. Il runtime di .NET Framework esiste solo
su Windows, quindi un progetto di test `net48` si compilerebbe ma non girerebbe nella CI su
Linux e macOS.

Le librerie sotto test sono:

| Progetto | Target | Testabile da questi test? |
|----------|--------|---------------------------|
| `Database` | `net48;net8.0` | ✅ tramite il target `net8.0` |
| `PollingClass` | `net48` (per ora) | ❌ i suoi tipi non sono testabili finché il progetto non è multi-target: il target `net8.0` arriva con **`X1.3c`** (ADR 0005) |

Conseguenza da non dimenticare: **`ParserFatture`, i flussi e i wrapper di stored procedure non
hanno copertura** finché `X1.3c` non è fatto. Il task `T2` (test unitari della logica pura) potrà
coprire la parte in `Database` e dovrà dichiarare la parte esclusa.

## I due progetti

### `PollingWeb.UnitTests` — 50 test, nessuna dipendenza esterna

| File | Copre | Note |
|------|-------|------|
| `AntiSqlInjectTests.cs` | il controllo antinjection di `Database` | **fissa la modifica di `X1.2`**: la sostituzione di `HttpUtility.UrlDecode` non deve cambiare il comportamento, essendo un controllo di sicurezza |
| `ValidationTests.cs` | `ValidaData`, `ValidaMail`, `IsNumeric` | include due test che **documentano difetti noti** (`C4` cultura, `C1` filtraggio troppo aggressivo) e uno saltato con motivazione (`C6`, `IsValidEmail(null)`) |

```bash
dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj
```

### `PollingWeb.IntegrationTests` — 16 test, richiedono SQL Server

| File | Copre |
|------|-------|
| `SchemaContractTests.cs` | il **contratto dello schema**: numero di tabelle, viste, funzioni, procedure, chiavi esterne e vincoli per ciascun database; la collocazione delle due tabelle spostate in `T0.12`; la semantica (fuorviante) di `Datini`/`Datfin`; la funzione `StripHTML` |
| `LeastPrivilegeTests.cs` | il **modello di privilegi minimi** di `T0.8`: l'utente applicativo può leggere, scrivere, eseguire procedure e attraversare `eProcurement`; **non** può eseguire DDL in nessuno dei tre database; non è `db_owner` né `sysadmin` |

```bash
# prepara l'istanza e lancia i test (consigliato)
POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] \
    ./scripts/test-integration.sh

# oppure, con l'istanza già pronta
POLLING_DB_$1=[REDACTED] dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj
```

## Salto automatico quando il database non c'è

I test di integrazione usano `RequiresDatabaseFactAttribute` / `RequiresAppUserFactAttribute`,
che impostano `Skip` se l'istanza non è raggiungibile, se il database `Polling` non esiste o se
manca `POLLING_APP_PASSWORD`.

Un test di integrazione che fallisce perché manca il database non segnala un difetto del codice,
e un fallimento che si impara a ignorare è peggio di un test assente. Il salto è **dichiarato
nell'output** e nel conteggio `Ignorati`, quindi resta visibile.

## Variabili d'ambiente

Le stesse usate da `scripts/db-deploy.sh`: nessuna credenziale nuova nel repository.

| Variabile | Uso |
|-----------|-----|
| `POLLING_DB_SERVER` | host e porta (default `localhost,14330`) |
| `POLLING_DB_USER` | utente amministrativo (default `sa`) |
| `POLLING_DB_PASSWORD` | password amministrativa — **obbligatoria** per i test di integrazione |
| `POLLING_APP_LOGIN` | login applicativo (default `PollingApp`) |
| `POLLING_APP_PASSWORD` | password applicativa — necessaria per `LeastPrivilegeTests` |

La stringa di connessione include `TrustServerCertificate=True`: è la conseguenza del fatto
`P15` (`Microsoft.Data.SqlClient` 5.x attiva `Encrypt` per default e contro il certificato del
container la connessione fallirebbe con un errore di handshake pre-login poco leggibile).

## Stato

`dotnet test` a livello di soluzione (`dotnet test PollingWeb.sln`) non è ancora possibile: la
soluzione contiene il sito WebForms, che non è compilabile in CI (`MSB4249`, fatto `P13`). I due
progetti si lanciano singolarmente; l'esecuzione unificata diventa possibile dopo `X1.7`.
