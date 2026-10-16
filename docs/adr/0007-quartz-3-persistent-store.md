# ADR 0007 — Quartz.NET 3 persistent store su SQL Server

- **Stato:** accettata e implementata (`X2.2`)
- **Data:** 2026-10-16
- **Decisione:** Quartz.NET 3.13.1, `JobStoreTX`, `SqlServerDelegate`, `Microsoft.Data.SqlClient` e serializzazione JSON; schema QRTZ su database `Quartz`.

## Contesto

I gateway Quartz introdotti in X1.3b1–b5 usavano API Quartz 2.1.2 (sincrone e non compatibili col target .NET 8). Lo scheduler deve poter registrare e recuperare trigger dopo un riavvio e riusare il provider SQL già adottato dal DAL (`Microsoft.Data.SqlClient`). Il repository conserva lo schema QRTZ e i test di privilegi minimi.

## Decisione

1. Aggiornare Quartz a 3.13.1; i metodi `IJob.Execute` restituiscono `Task`.
2. Configurare `QuartzSchedulerFactory` su `JobStoreTX`/`SqlServerDelegate`; provider `MicrosoftDataSqlClient`, tabella-prefix `QRTZ_`, `useProperties=true`, serializer JSON. Il clustering resta disabilitato: singola istanza per questa fase. Misfire threshold 60 secondi.
3. Risolvere la connessione in quest'ordine: argomento del factory, `POLLING_QUARTZ_CONNECTION`, `ConnectionStrings:QuartzConnectionString`. Nessuna credenziale versione-controllata.
4. Per non cambiare le firme legacy dei servizi, gli adapter attendono le operazioni asincrone Quartz 3 ai confini sincroni. Il refactoring asincrono dei flussi è fuori da X2.2.
5. Lo schema `sql/Quartz/quartz-schema.sql` contiene solo DDL, 11 tabelle QRTZ e foreign keys. Rimossi INSERT storici e payload binari di job; deploy non reimporta configurazioni/job di un'installazione precedente. Backup/migrazione di eventuali job reali è responsabilità operativa pre-deploy, non automatizzata.
6. Il test d'integrazione crea dati sintetici, registra trigger, riavvia lo scheduler e verifica il recupero. Usa la connessione utente applicativo del fixture; pulisce il job di test.

## Evidenza

- `dotnet build PollingClass/PollingClass.csproj -f net48 --nologo -v quiet` — Linux: 0 errori, 5 warning preesistenti.
- `dotnet test tests/PollingWeb.UnitTests/PollingWeb.UnitTests.csproj --nologo -v quiet` — 98 passati, 2 ignorati (scenario DB persistente è nel progetto d'integrazione).
- `POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] ./scripts/test-integration.sh` — schema, test privilegi, Quartz restart/recovery; 16/16 passati.
- Windows/macOS non verificati in locale.

## Conseguenze

Quartz 3 supporta i target moderni e l'ADO store persistente, prerequisito per X1.3c e X2.1. Le API restano sincrone nei gateway legacy e il formato dati usa JSON; l'applicazione non legge i payload binari storici. Deploys devono fornire il database Quartz aggiornato e una connessione con i privilegi minimi elencati in `sql/Quartz/Security/Permissions.sql`.
