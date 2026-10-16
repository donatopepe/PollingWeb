# SECURITY FINDINGS — PollingWeb

> Inventario dei segreti e dei dati sensibili presenti nel repository.
> **Task:** `T0.1` (inventario) · `T0.10` (redazione + riscrittura storia) · **Data:** 2026-10-16
>
> ⚠️ **Le credenziali elencate qui sotto vanno considerate compromesse e ruotate.**
>
> **Stato di esposizione accertato il 2026-10-16** (`gh repo view`, `gh api`): la repository
> `github.com/donatopepe/PollingWeb` è **privata**, con il solo proprietario come
> collaboratore, 0 fork, nessun webhook, nessuna deploy key. I segreti quindi **non sono
> stati esposti al pubblico**, e una precedente versione di questo documento lo affermava
> erroneamente.
>
> La rotazione resta comunque **obbligatoria prima della pubblicazione**, perché:
>
> 1. la repository è destinata a diventare pubblica: senza rotazione, la pubblicazione
>    esporrebbe credenziali ancora valide;
> 2. non è verificabile a posteriori che il contenuto non sia stato copiato (clone locali,
>    backup, macchine di sviluppo) nel periodo 2023-2026;
> 3. le credenziali erano deboli **a prescindere** dalla storia Git: password uguale al
>    nome utente, `CHECK_POLICY = OFF`, `SID` esplicito che le rendeva replicabili;
> 4. la storia Git rendeva il leak **banale**: sarebbe bastato un `git log -p` dopo la
>    pubblicazione, quindi il rischio era reale, solo non ancora consumato.
>
> 🔒 **Questo documento è redatto.** I valori originali (password, hostname, IP) **non sono
> più presenti nel repository né nella sua storia**: sono conservati solo in un record di
> audit offline, la cui posizione e le cui istruzioni di accesso sono descritte in
> `docs/RELEASING.md` (§ record di audit). I placeholder in questo documento hanno la forma
> `<REDACTED-…>` e `<INTERNAL-…>`.

---

## 1. Sintesi

| ID | Tipo | Gravità | Occorrenze | Dove (file, non contenuto) |
|----|------|---------|-----------|----------------------------|
| S1 | Password SQL LOGIN in chiaro | **Critica** | 2 | `<DB_SCHEMA>/Security/<LEGACY-APP-LOGIN>_1.sql` in entrambi i database project |
| S2 | Credenziali DB applicative in chiaro | **Critica** | 22 | `Web.config`, `Database/app.config`, `Database/Properties/Settings.*`, `*/bin/*.config` |
| S3 | Credenziali DB legacy (utente `<REDACTED-LEGACY-LOGIN>`) | **Alta** | 1 | `PollingClass/ParserFatture.cs` (riga in commento) |
| S4 | Hostname/IP interni | **Media** | 87 (17 file) | `Web.config`, `Database/app.config`, `PollingWeb.sln`, `PublishProfiles/*`, un file C# orfano (rimosso in `T0.6`), artefatti `*/bin` e `*/obj` |
| S5 | Dati reali di terzi (PII/anagrafiche) | **Alta** | 3 file | `SC00G100_inizio.TXT`, `script_eprocurement_donato.sql`, `BDG_MCC.xlsx` |
| S6 | Credenziali SMTP placeholder non funzionanti | **Bassa** | 2 | `PollingClass/MailSede.cs` |
| S7 | Path assoluti interni (leak di struttura) | **Bassa** | 3 | `App_Data/PublishProfiles/FolderProfile.pubxml`, `*/obj/*FileListAbsolute.txt` |
| S8 | Marchio/logo di terze parti | **Media (legale)** | 1 | `PollingWeb/images/<bank-logo>.png` |

---

## 2. Dettaglio

### S1 — Password SQL LOGIN in chiaro (Critica)

Due script di creazione login contenevano la password in chiaro:

```
<DB_SCHEMA>/Security/<LEGACY-APP-LOGIN>_1.sql
CREATE LOGIN [<LEGACY-APP-LOGIN>]
    WITH PASSWORD = N'<REDACTED-LOGIN-PWD-A>',
    SID = 0x<REDACTED>, DEFAULT_LANGUAGE = [Italiano],
    CHECK_POLICY = OFF;
```

Note aggravanti (indipendenti dal valore della password):

- `CHECK_POLICY = OFF` → nessuna policy di scadenza/complessità.
- `SID` esplicito → la password era replicabile su qualunque istanza.
- `GRANT CONNECT` a livello server.
- La password coincideva con il nome utente.

**Stato al 2026-10-16:** script rinominati in `Security/01-create-app-login.sql`; password
fornita da variabile `sqlcmd` senza default; `CHECK_POLICY = ON`; `SID` rimosso (task `T0.2`).

### S2 — Credenziali DB applicative (Critica)

Le stesse credenziali comparivano in 22 punti, con password uguale al nome utente:

| File | Contenuto (redatto) |
|------|---------------------|
| `PollingWeb/Web.config` | 4 connection string: `eProcurement`, `Polling`, `Quartz`, variante `DB` |
| `Database/app.config` | 2 connection string (`Polling`, `eProcurement`) |
| `Database/Properties/Settings.Designer.cs` | attributi `DefaultSettingValueAttribute` con le stesse stringhe |
| `Database/Properties/Settings.settings` | `DesignTimeValue` + `Value` |
| `Database/bin/{Debug,Release}/Database.dll.config` | artefatti di build, **tracciati in Git** |
| `PollingClass/bin/{Debug,Release}/Database.dll.config` | idem |
| `PollingWeb/Bin/Database.dll.config` | idem |

Host: `<INTERNAL-SQL-HOST>` · login: `<LEGACY-APP-LOGIN>` · password: `<REDACTED-APP-PWD>`

**Stato al 2026-10-16:** sostituite da placeholder `${POLLING_DB_*}` (`T0.2`); artefatti
`bin/obj` de-tracciati (`T0.5`).

### S3 — Credenziali legacy in commento (Alta)

```
PollingClass/ParserFatture.cs
//CONN_STRING = "Provider=SQLOLEDB.1;Data Source=<INTERNAL-SQL-HOST-2>;" +
//  " Initial Catalog=eProcurement;" + "user id = '<REDACTED-LEGACY-LOGIN>';" +
//  "password = '<REDACTED-LEGACY-PWD>'";
```

**Stato al 2026-10-16:** riga rimossa (`T0.2`).

### S4 — Hostname/IP interni (Media)

| Valore (redatto) | Dove compariva |
|------|--------|
| `<INTERNAL-SQL-HOST>` | `PollingWeb.sln` (URL TFS), `Web.config`, `Database/app.config` |
| `<INTERNAL-SQL-HOST-2>` | `PollingClass/ParserFatture.cs` |
| `<INTERNAL-SQL-HOST-3>` | un file C# orfano, non compilato (rimosso in `T0.6`) |
| `<INTERNAL-SMTP-HOST>` | `PollingWeb/Web.config` |
| `<INTERNAL-PUBLISH-PATH>` | `App_Data/PublishProfiles/FolderProfile.pubxml`, `*/obj/*FileListAbsolute.txt` |

Ripartizione delle 87 occorrenze:

```
58  */obj/*.csproj.FileListAbsolute.txt   (artefatti di build)
16  */bin/*/Database.dll.config           (5 file x 2 + 6 varianti)
 4  Database/Properties/Settings.settings
 2  PollingWeb/Web.config (host SQL x2 + SMTP x1 + TFS)
 2  PollingWeb.sln
 2  Database/Properties/Settings.Designer.cs
 2  Database/app.config
 2  file C# orfano (rimosso in T0.6)
 1  PollingClass/ParserFatture.cs
 1  PollingWeb/App_Data/PublishProfiles/FolderProfile.pubxml
```

Esponeva topologia di rete, naming interno e struttura di un istituto bancario.

**Stato al 2026-10-16:** redatti (`T0.2`); artefatti de-tracciati (`T0.5`).

### S5 — Dati reali di terze parti (Alta)

| File | Dimensione | Contenuto |
|------|-----------|-----------|
| `script_eprocurement_donato.sql` | 12,7 MB | 15.732 istruzioni `INSERT` su schema `eProcurement` |
| `script_polling_donato.sql` | 1,3 MB | dump completo schema `Polling` |
| `SC00G100_inizio.TXT` | 254 KB | anagrafica fornitori con ragione sociale, indirizzo e **P.IVA / codici fiscali** |
| `BDG_MCC.xlsx` | 10,7 KB | dati di budget |
| `_047_1047_oda_input.#00`, `_063_1063_rda_input.#00` | — | tracciati SAP reali |

> Dati di titolarità di terzi. La pubblicazione può configurare violazione della disciplina
> di protezione dei dati personali e degli obblighi di riservatezza contrattuale.

**Stato al 2026-10-16:** da rimuovere — task `T0.3` (non ancora eseguito).

### S6 — SMTP (Bassa)

`MailSede` costruiva `NetworkCredential` con valori fittizi (`"username"` / `"password"`):
l'autenticazione SMTP non era mai realmente implementata. Non è un leak, ma un difetto
funzionale. Da correggere in `C10`.

### S7 — Path assoluti (Bassa)

`App_Data/PublishProfiles/FolderProfile.pubxml` puntava a `<INTERNAL-PUBLISH-PATH>`;
i `*/obj/*FileListAbsolute.txt` contenevano lo stesso prefisso.

**Stato al 2026-10-16:** neutralizzato (`T0.2`); artefatti de-tracciati (`T0.5`).

### S8 — Marchio di terze parti (Media, legale)

`PollingWeb/images/<bank-logo>.png` — logo registrato di un istituto di credito.
Non ridistribuibile con licenza MIT. Da rimuovere in `T0.4`, con attribuzione storica in `NOTICE`.

---

## 3. Riproduzione dell'evidenza

I pattern originali **non sono riportati in questo documento**: sono essi stessi informazione
sensibile (hostname, IP, login, password rimossi) e risiedono unicamente nel **record di audit
offline** descritto in `docs/RELEASING.md`. Chi esegue la verifica deve recuperare il file
`secret-patterns.txt` da quella posizione e usarlo come sorgente dei pattern.

```bash
cd /home/public/PollingWeb

# AUDIT_DIR = directory del record di audit offline (NON versionata, permessi 700).
# Contiene secret-patterns.txt con un pattern per riga.
PATTERNS="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"

# 1) Nessun file tracciato contiene i valori rimossi (deve essere vuoto)
git grep -nE "$PATTERNS" -- .

# 2) Nessun blob di NESSUN commit raggiungibile contiene i valori (deve essere vuoto).
#    Scorre l'intera object database raggiungibile, non solo il working tree.
git rev-list --objects --all \
  | awk '{print $1}' \
  | git cat-file --batch-check='%(objectname) %(objecttype)' \
  | awk '$2=="blob"{print $1}' \
  | git cat-file --batch \
  | grep -aoE "$PATTERNS" | sort -u

# 3) La storia da pubblicare deve contenere un solo commit (orfano, sanificato).
git rev-list --all --count        # atteso: 1

# 4) Nessun oggetto irraggiungibile residuo nel repository locale.
git count-objects -v              # atteso: count=0, garbage=0, size 0
```

> La verifica (2) è quella che conta: `git grep` guarda solo il working tree, mentre gli
> oggetti della storia restano nel database Git finché non vengono espirati con
> `git reflog expire --expire=now --all && git gc --prune=now`.

---

## 4. Soglia di superamento

- [x] ogni occorrenza classificata con gravità e posizione (`T0.1`);
- [x] comandi di riproduzione disponibili;
- [x] mitigazione definita per ciascun ID;
- [x] S1, S2, S3, S4, S7 azzerati nei file tracciati (`T0.2`);
- [x] valori originali rimossi da tutti i commit raggiungibili (`T0.10`);
- [ ] S5 (`T0.3`) e S8 (`T0.4`) ancora da eseguire;
- [ ] S6 (`C10`).

---

## 5. Piano di rotazione credenziali (operativo — a carico dell'ente)

> Da eseguire **prima** di qualunque pubblicazione definitiva. Azioni non eseguibili da
> questo repository: richiedono accesso all'infrastruttura.

1. **S1 — Login SQL applicativo**
   - `ALTER LOGIN [<login>] WITH PASSWORD = N'<nuovo-segreto-casuale>'` su **ogni** istanza
     (`Polling`, `eProcurement`, `Quartz`).
   - Riabilitare `CHECK_POLICY = ON` e `CHECK_EXPIRATION = ON`.
   - Se il login legacy non è più necessario, disabilitarlo e creare un login per applicazione
     con permessi minimi (`EXECUTE` sulle stored procedure usate, CRUD sulle tabelle
     applicative, nessun `db_owner`, nessun ruolo server).
2. **S2 — Connection string applicative**
   - Ruotare la password e conservare il segreto **fuori dal repository** (variabili
     d'ambiente, `aspnet_regiis -pe` per la sezione `connectionStrings`, o secret store).
   - Sostituire l'utente condiviso con un utente dedicato a privilegi minimi.
3. **S3 — Login legacy `<REDACTED-LEGACY-LOGIN>`**
   - Verificare se esiste ancora sull'istanza `<INTERNAL-SQL-HOST-2>`; se sì disabilitarlo
     o ruotare la password e convertirlo a sola lettura.
4. **S4 — Host e path**
   - Nessuna azione infrastrutturale; rimozione dal codice completata.
5. **S5 — Dati di terzi**
   - Cancellare i file dal repository pubblico; confermare con il titolare del trattamento
     la necessità di notifica se i dati erano riconducibili a persone fisiche.
6. **Storia Git**
   - La riscrittura **locale** è stata eseguita (`T0.10`): la storia non contiene più alcun
     valore sensibile (verificato su 164 blob, 0 corrispondenze).
   - **Prima di pubblicare** il repository va sostituito il contenuto remoto: la storia con
     i segreti è ancora quella presente su GitHub (`master` @ `6b01bfa`). La procedura è in
     `docs/RELEASING.md` §2.
   - **Azione obbligatoria:** ruotare i segreti (punti 1–3) **prima** della pubblicazione.
   - **Azione consigliata:** abilitare *secret scanning* e *push protection* sul repository
     (richiede piano GitHub Pro o repository pubblico).

---

## 6. Controlli automatici introdotti (tracciati in `PLAN.md`)

| Controllo | Task |
|-----------|------|
| `.gitignore` per artefatti di build | T0.5 ✅ |
| Configurazione esternalizzata + file `.example` | T0.2 ✅ |
| Rimozione dati reali + `samples/` sintetici | T0.3 |
| De-branding | T0.4 |
| Storia Git pulita con backup `git bundle` | T0.10 ✅ |
| Segreti solo da variabili d'ambiente | X1.4 |
| `gitleaks`/`trufflehog` in CI | R6 |
| Scan PII e licenze in checklist di rilascio | R6 |
