# Modalità legacy WebForms

`PollingWeb/` è un ASP.NET Website (WebForms, `CodeFile` e controlli generati dal designer), mantenuto esclusivamente per installazioni IIS/.NET Framework 4.8 esistenti. Non fa parte del motore cross-platform né dei build/test CI predefiniti. Il port UI è previsto in `X4.1`–`X4.4` (Razor Pages); non si dichiarano portate o supportate le 11 pagine legacy.

## Perché la build standard lo esclude

Il formato Website non è un `.csproj` SDK-style: la sua compilazione dipende dal compilatore ASP.NET del MSBuild .NET Framework e dalla generazione di `.designer.cs`. `dotnet build PollingWeb.sln` da .NET SDK su Linux termina attualmente con `MSB4249`. Non è corretto sopprimere l'errore o chiamare verde la soluzione; eseguire i progetti SDK registrati separatamente. La cartella WebForms resta presente nella soluzione Visual Studio per la modalità legacy.

## Compilazione legacy

Per la compilazione IIS utilizzare Visual Studio su Windows con il workload ASP.NET/.NET Framework 4.8 Developer Pack e i riferimenti/configurazione locali. Aprire `PollingWeb.sln` e compilare il Website con il compilatore ASP.NET installato. La compilazione non è automatizzata da `dotnet build` su Linux/macOS e non è verificata in questa release dal runner Linux. Non committare configurazioni o segreti locali.

Per verificare le librerie e i test cross-platform, usare invece i comandi `dotnet build` per `Database` e `PollingClass` (entrambi i TFM), più i progetti test elencati in `.github/workflows/ci.yml`.
