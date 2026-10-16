# ADR 0004 — Layout di build, multi-target e revisione dell'ordine della fase X1

- **Stato:** accettata
- **Data:** 2026-10-16
- **Contesto del piano:** task `X1.1`, `X1.2`, `X1.3`, `X1.5`
- **Nota sulla numerazione:** `0002` e `0003` sono riservati dal piano a Quartz (`X2.2`) e alla
  compatibilità SOAP (`X3.3`).

---

## 1. Problema

Il piano si contraddice su `X1.1`, che è il primo task della fase cross-platform.

**La DoD di `X1.1` chiede due cose non ottenibili in quel punto:**

1. una soluzione contenente `PollingWorker`, `PollingApi`, `PollingWeb.Core` e `tests/*` —
   progetti che vengono **creati** da `X2.1`, `X3.1`, `X4.1` e `T1`, cioè dopo;
2. l'evidenza `dotnet build PollingWeb.sln` → **0 errori**, mentre i fatti `P13` e `P14` dello
   stesso piano dicono che la soluzione non è compilabile finché non sono fatti `X1.1`–`X1.3`
   (`MSB3644`, targeting pack `v4.5` mancanti) e `X1.7` (`MSB4249`, compilatore ASP.NET solo su
   Windows).

Verificato il 2026-10-16:

```
$ for p in PollingWorker PollingApi PollingWeb.Core tests; do [ -e "$p" ] && echo esiste || echo ASSENTE; done
ASSENTE (tutti e quattro)

$ dotnet build PollingWeb.sln
2 errori: MSB4249 sul sito WebForms, MSB3644 sui progetti classici
```

Un task la cui evidenza non può passare non è verificabile: finirebbe marcato `[x]` su
un'affermazione falsa, oppure resterebbe `[~]` per sempre bloccando la fase. Entrambe le
alternative sono peggiori di una revisione esplicita.

**Secondo problema, minore.** La DoD di `X1.2` contiene due imprecisioni: chiede «nessun
`WindowsBase`», ma quel riferimento è in `PollingClass` (quindi riguarda `X1.3`), e rimanda a
`X1.5` per l'astrazione di `SqlClient` — un riferimento in avanti che rende ambiguo cosa
`X1.2` debba consegnare.

**Terzo problema, emerso dalla verifica.** La DoD di `X1.5` prescrive **due** provider
(`Microsoft.Data.SqlClient` su net8.0, `System.Data.SqlClient` su net48) dietro un adapter.
La verifica empirica mostra che **un solo provider funziona su entrambi i target**:

```
$ dotnet build probe.csproj          # TargetFrameworks net48;net8.0
Compilazione completata.  Avvisi: 0  Errori: 0
      con Microsoft.Data.SqlClient 5.2.2 + SqlDataAdapter + SqlCommandBuilder
      + System.Configuration.ConfigurationManager 8.0.0
```

Mantenere due provider significherebbe costruire un'astrazione per un problema che non esiste.

## 2. Decisione

### 2.1 `X1.1` viene ristretto a ciò che può consegnare

`X1.1` consegna **lo scheletro di build**:

- `Directory.Build.props` — `LangVersion`, `Nullable`, `Deterministic`, versione;
- `Directory.Packages.props` — Central Package Management;
- `Directory.Build.targets` — regole comuni;
- soluzione con i **progetti esistenti**.

Le due richieste non ottenibili vengono spostate:

| Richiesta | Dove va | Perché |
|---|---|---|
| `PollingWorker`, `PollingApi`, `PollingWeb.Core`, `tests/*` nella soluzione | `X2.1`, `X3.1`, `X4.1`, `T1` | sono quei task a **creare** i progetti: registrarli nella soluzione è parte della loro DoD, non di un task che li precede |
| `dotnet build PollingWeb.sln` → 0 errori | `X1.6` (CI multi-OS) e `X1.7` | è il primo punto in cui l'affermazione è vera; fino ad allora è un obiettivo, non un'evidenza |

L'evidenza di `X1.1` diventa verificabile **subito**:

```bash
# i props sono importati ed efficaci (funziona anche su csproj classici:
# Directory.Build.props entra da Microsoft.Common.props)
dotnet msbuild Database/Database.csproj -getProperty:LangVersion -getProperty:Version

# la soluzione elenca i progetti attesi
dotnet sln list
```

### 2.2 `X1.1` si sposta **dopo** `X1.2` e `X1.3`

Ordine rivisto della fase `X1`:

```
X1.2 (Database multi-target)  →  X1.3 (PollingClass multi-target)  →  X1.1 (props + soluzione)
  →  X1.4 (config astratta)  →  X1.5 (provider dati)  →  X1.6 (CI multi-OS)  →  X1.7 (legacy)
```

Prima `X1.2`/`X1.3` perché i props hanno senso solo su progetti SDK-style: introdurli prima
significherebbe dichiararli verificati senza poterli esercitare, e riscriverli due volte.
Dopo `X1.1`, la CI di `X1.6` può finalmente pretendere la soluzione verde.

### 2.3 Correzioni alle DoD di `X1.2` e `X1.5`

- `X1.2`: «nessun `WindowsBase`» si sposta in `X1.3` (il riferimento è di `PollingClass`);
  l'astrazione del provider si sposta in `X1.5`, mentre `X1.2` usa il provider direttamente.
- `X1.5`: **un solo provider**, `Microsoft.Data.SqlClient`, su entrambi i TFM; l'adapter resta,
  ma con lo scopo di isolare il DAL da un futuro cambio di provider, non di scegliere fra due.

### 2.4 Eliminazione dell'ultima dipendenza da `System.Web` nella DAL

`AntiSqlInject` usa `HttpUtility.UrlDecode`. Invece di aggiungere `<Reference Include="System.Web" />`
solo per net48, la dipendenza viene **rimossa** con una funzione equivalente:

```csharp
static string UrlDecode(string input) =>
    input == null ? null : Uri.UnescapeDataString(input.Replace('+', ' '));
```

`HttpUtility.UrlDecode` converte `+` in spazio, `Uri.UnescapeDataString` no: la differenza è
replicata esplicitamente. Equivalenza verificata su 8 casi, inclusi payload di injection e
UTF-8 percent-encoded:

```
=  a%20b+c                      'a b c'          'a b c'
=  1%27%20or%20%271%27%3D%271   '1' or '1'='1'   '1' or '1'='1'
=  SeLeCt+*+FrOm                'SeLeCt * FrOm'  'SeLeCt * FrOm'
=  %3Bdrop%20table              ';drop table'    ';drop table'
=  a+b                          'a b'            'a b'
=  50%25                        '50%'            '50%'
=  caf%C3%A9                    'café'           'café'
=  %2F%2E%2E                    '/..'            '/..'
differenze: 0
```

Il comportamento dell'antinjection è **invariato**, ed è la parte che non deve cambiare: è un
controllo di sicurezza.

## 3. Conseguenze

**Positive**

- Ogni task della fase `X1` ha un'evidenza che può effettivamente passare.
- Nessuna astrazione costruita per un problema inesistente (i due provider).
- La DAL perde l'ultima dipendenza da `System.Web`: il porting a net8.0 non richiede più
  nessun riferimento all'assembly web su net48.

**Negative / da gestire**

- La numerazione dei task non segue più l'ordine di esecuzione dichiarato in origine: chi legge
  §4 deve tenere conto dell'ordine rivisto. Mitigazione: §4 è aggiornato e questa ADR lo motiva.
- `X1.2` usa `Microsoft.Data.SqlClient` prima che `X1.5` introduca l'adapter: per un'iterazione
  la DAL dipende direttamente dal provider. È il debito che `X1.5` salda.
- `X1.1` non dimostra più, da solo, che la soluzione compila: quella prova si sposta in `X1.6`.
  È una riduzione di garanzia **apparente**: prima era una garanzia impossibile.

## 4. Alternative considerate

| Alternativa | Perché non scelta |
|---|---|
| Lasciare `X1.1` com'è e marcarlo `[~]` fino a `X1.3` | Un task `[~]` in testa alla fase blocca l'avanzamento: il loop riprende sempre il primo non completato, quindi `X1.2`/`X1.3` non verrebbero mai iniziati |
| Marcarlo `[x]` accettando l'evidenza parziale | Sarebbe un'affermazione falsa nel registro: la soluzione non compila |
| Fare tutto in `X1.1` (props + conversione dei progetti + soluzione) | Un task unico che tocca tre progetti e li converte non è verificabile in modo granulare: un fallimento non direbbe quale parte è rotta. È esattamente ciò che la regola «un task = un cambio» evita |
| Creare subito i progetti vuoti `PollingWorker`/`PollingApi`/`PollingWeb.Core` per soddisfare la DoD | Sarebbe compiacenza verso la DoD: progetto vuoto non è progetto. Verrebbero comunque riempiti da `X2.1`/`X3.1`/`X4.1`, con il rischio che restino stub dimenticati |
| Due provider come prescritto in `X1.5` | Duplicherebbe percorsi di test per una compatibilità già dimostrata inutile |

## 5. Verifica

```
# i props sono efficaci (X1.1)
dotnet msbuild Database/Database.csproj -getProperty:LangVersion,Version

# entrambi i target compilano (X1.2)
dotnet build Database/Database.csproj -f net48
dotnet build Database/Database.csproj -f net8.0

# un solo provider, entrambi i TFM
dotnet build probe.csproj    # TargetFrameworks net48;net8.0 -> 0 errori
```
