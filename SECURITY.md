# Security Policy — PollingWeb

Questo documento descrive:

1. come **segnalare** una vulnerabilità;
2. la policy di **divulgazione coordinata**;
3. l'**avviso di sicurezza storico** del progetto e la **rotazione delle credenziali**
   obbligatoria prima di qualunque pubblicazione;
4. i controlli che chi contribuisce deve eseguire **prima di ogni commit**.

---

## 1. Versioni supportate

| Versione | Stato | Note |
|----------|-------|------|
| `main` (sviluppo) | ✅ supportata | riceve correzioni di sicurezza |
| `< 1.0` (storico, precedente alla pubblicazione) | ❌ non supportata | storia Git precedente: **non usare**, contiene credenziali rimosse |

Non esiste ancora una release `v1.0.0`: fino a quel momento solo `main` è supportata.

---

## 2. Segnalare una vulnerabilità

**Non aprire una issue pubblica.** Una issue pubblica rende la vulnerabilità sfruttabile
prima che esista una correzione.

Usa uno di questi canali privati:

1. **GitHub Security Advisories** — scheda *Security* → *Report a vulnerability* della
   repository (canale preferito: tiene traccia della comunicazione e permette di pubblicare
   l'advisory in modo coordinato);
2. in alternativa, contatta privatamente il manutentore indicato nel `README.md`.

### Cosa includere nella segnalazione

- descrizione del problema e dell'impatto che immagini;
- passi di riproduzione minimi e deterministici (comandi, input, versione);
- la versione o il commit su cui hai verificato;
- eventuale prova di concetto, **senza dati reali di terzi**;
- se lo desideri, come vuoi essere accreditato.

### Tempi di risposta (obiettivo)

| Fase | Obiettivo |
|------|-----------|
| Presa in carico | 5 giorni lavorativi |
| Valutazione iniziale e severità | 10 giorni lavorativi |
| Correzione per problemi critici | 30 giorni, o piano concordato |
| Pubblicazione dell'advisory | dopo la correzione, concordata con chi segnala |

Sono obiettivi, non un contratto: il progetto è manutenuto a tempo parziale. Se non ricevi
risposta entro 10 giorni lavorativi, sollecita sullo stesso canale.

### Fuori perimetro

- configurazioni locali errate non riproducibili (es. credenziali di un proprio ambiente);
- problemi che richiedono accesso fisico all'host;
- vulnerabilità di dipendenze di terze parti prive di impatto dimostrabile su questo codice:
  segnalale al progetto a monte e, se vuoi, apri qui una issue **non** riservata.

---

## 3. Divulgazione coordinata

- Chiediamo un periodo di **90 giorni** prima della divulgazione pubblica, o meno se la
  correzione è disponibile prima.
- Ci impegniamo a: confermare la ricezione, tenerti aggiornato, citarti nell'advisory se lo
  desideri, non intraprendere azioni legali per ricerche condotte in buona fede.
- Ti chiediamo di: non accedere a dati altrui, non degradare il servizio, non divulgare prima
  del termine concordato.

---

## 4. ⚠️ Avviso di sicurezza storico (leggere prima di installare o pubblicare)

Il progetto nasce da un'applicazione interna e la sua storia Git conteneva, nel commit
radice, **credenziali di produzione, dati reali di terzi e riferimenti all'organizzazione**
per cui era stato sviluppato.

**Stato accertato il 2026-10-16:**

| Aspetto | Stato |
|---------|-------|
| Visibilità della repository | **privata** (`visibility: private`), unico collaboratore il proprietario, 0 fork |
| Esposizione pubblica dei segreti | **nessuna** |
| Storia Git del branch `main` | **riscritta**: 173 blob scansionati, 0 contengono valori sensibili; 0 nei messaggi di commit |
| `master` (storia precedente) | eliminato dal remoto |
| Oggetti orfani sul remoto | ⚠️ **ancora recuperabili per SHA** — GitHub non compatta immediatamente |
| Rotazione delle credenziali | ❌ **non ancora eseguita** |

### Cosa significa in pratica

1. **Prima di pubblicare la repository**, ruotare le credenziali: senza rotazione, la
   pubblicazione esporrebbe credenziali ancora valide. Procedura operativa completa in
   [`docs/SECURITY-FINDINGS.md`](docs/SECURITY-FINDINGS.md) §5.
2. La rotazione è obbligatoria **anche se i segreti non sono mai stati pubblici**, per tre
   ragioni indipendenti:
   - la pubblicazione è imminente e li esporrebbe;
   - le credenziali erano deboli **di per sé** (password uguale al nome utente,
     `CHECK_POLICY = OFF`, `SID` esplicito che le rendeva replicabili su qualsiasi istanza);
   - non è verificabile a posteriori che non ne esistano copie (clone locali, backup,
     macchine di sviluppo) nel periodo 2023–2026.
3. Il commit precedente **resta leggibile per SHA** dal remoto. Per chiudere il punto
   servono: la rotazione (mitigazione garantita), una richiesta a GitHub Support per la
   compattazione degli oggetti orfani, oppure la cancellazione e ricreazione della repository.
   Dettagli in [`docs/RELEASING.md`](docs/RELEASING.md) §2.

L'inventario completo dei problemi trovati, con gravità e posizione, è in
[`docs/SECURITY-FINDINGS.md`](docs/SECURITY-FINDINGS.md).

---

## 5. Rotazione delle credenziali — riepilogo

Procedura dettagliata e comandi: [`docs/SECURITY-FINDINGS.md`](docs/SECURITY-FINDINGS.md) §5.

Sintesi delle azioni minime prima di pubblicare:

1. **Login SQL applicativo** — `ALTER LOGIN ... WITH PASSWORD = ...` su ogni istanza
   (`Polling`, `eProcurement`, `Quartz`); riabilitare `CHECK_POLICY` e `CHECK_EXPIRATION`;
   sostituire l'utente condiviso con uno a **privilegi minimi** (solo `EXECUTE` sulle stored
   procedure usate e CRUD sulle tabelle applicative).
2. **Stringhe di connessione** — ruotare la password e conservarla **fuori dal repository**:
   variabili d'ambiente, `aspnet_regiis -pe` per la sezione `connectionStrings`, o un secret
   store. I file versionati contengono solo placeholder `${POLLING_DB_*}`.
3. **SMTP** — l'invio non implementa realmente l'autenticazione: configurare utente e
   password reali e verificare con un server di prova (task `C10` del piano).
4. **Dati di terzi** — l'inventario elenca i file rimossi; verificare con il titolare del
   trattamento se serve una notifica.

---

## 6. Regole per chi contribuisce

Vincoli non negoziabili (ripresi da `PROMPT.md` e `PLAN.md`):

1. **Nessun segreto nel repository**, mai: né nei file, né nei messaggi di commit, né negli
   identificatori di codice, database, progetto o file. I pattern da ricercare sono nel
   record di audit offline (posizione in `docs/RELEASING.md`): i documenti versionati
   rimandano a quel file, non ne contengono i valori.
2. **Nessun dato reale** di terzi. Solo dati sintetici in `samples/`, con segnaposto evidenti
   (es. P.IVA `00000000001`).
3. **Nessun placeholder "mascherato"**: una classe di caratteri che lascia leggibile il
   frammento (`host[i]nterno`) **non** è una redazione, è un leak.

### Controllo prima di ogni commit

```bash
cd <repo>
PATTERNS="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"   # AUDIT_DIR: docs/RELEASING.md §1

git grep -nE "$PATTERNS" -- .        # deve essere vuoto
```

### Controllo prima di ogni push (o su richiesta)

```bash
# nessun blob di nessun commit raggiungibile deve contenere i valori
git rev-list --objects --all \
  | awk '{print $1}' \
  | git cat-file --batch-check='%(objectname) %(objecttype)' \
  | awk '$2=="blob"{print $1}' \
  | git cat-file --batch \
  | grep -aoE "$PATTERNS" | sort -u
```

### ⚠️ Trappola da conoscere

`git fetch origin <sha-vecchio>` **reintroduce i segreti nel `.git` locale**. Dopo una
verifica di questo tipo, ripulire:

```bash
rm -f .git/FETCH_HEAD
git reflog expire --expire=now --all
git gc --prune=now
```

### Vulnerabilità note nel codice, da non reintrodurre

| Problema | Dove | Task |
|----------|------|------|
| Concatenazione SQL in ingresso utente | `Database/DataType.cs` (`FiltroCampi.BuildWhere`), pagine con `SqlDataSource` | `C1`–`C3` |
| Parsing di date/numeri dipendente dalla cultura | vari `Convert.ToDateTime(string)` | `C4` |
| Stato statico mutabile nella connessione DB | `Database/DatabaseInfo.cs` | `C5` |
| Eccezioni inghiottite (`catch { }`) | vari | `C6` |
| Percorsi assoluti di macchina nel codice | vari | `C8` |
| Autenticazione SMTP non implementata | `PollingClass/MailSede.cs` | `C10` |
| Dipendenza copyleft (`LinqToExcel` → `Remotion`, LGPL) | `PollingClass` | ✅ rimossa in `X1.3`: era codice morto |

---

## 7. Hardening consigliato per chi installa

- abilitare **secret scanning** e **push protection** sulla repository (richiede GitHub Pro
  o repository pubblico);
- abilitare l'alerting delle dipendenze;
- eseguire SQL Server e l'applicazione su reti separate, con l'utente applicativo a privilegi
  minimi e nessun accesso interattivo;
- non esporre le API (`PollingApi`) su Internet senza autenticazione: gli endpoint di import
  elaborano file e scrivono sul database;
- servire il sito solo in HTTPS e con `customErrors` in modalità `RemoteOnly` o `On`
  (il valore storico `Off`, ancora presente nella configurazione legacy, espone gli stack
  trace — task `C7`).
