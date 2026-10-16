# RELEASING — PollingWeb

Procedura di pubblicazione e gestione del **record di audit** dei valori rimossi dal
repository. Questo documento è richiesto dai task `T0.10`, `R6` e `R7` del piano.

---

## 1. Record di audit (valori rimossi)

Durante la sanificazione (task `T0.2`, `T0.3`, `T0.4`, `T0.10`) sono stati rimossi dal
repository segreti, informazioni interne e riferimenti a marchi di terzi. I **valori
originali non esistono più** in nessun oggetto della storia Git: sono conservati in una
**directory di audit offline**, fuori dal repository.

### Contenuto della directory di audit

| File | Contenuto |
|------|-----------|
| `full-history.bundle` | storia Git completa precedente alla riscrittura, verificata con `git bundle verify` |
| `REDACTED-VALUES.md` | valori originali (password, hostname, IP, marchio) con il contesto d'uso |
| `secret-patterns.txt` | un pattern per riga: è la sorgente usata dalle verifiche di pulizia |
| `README.md` | istruzioni di ispezione |

### Requisiti di custodia

- permessi `700` sulla directory, `600` sui file;
- **fuori** dal repository e da qualunque sincronizzazione cloud condivisa;
- **mai** in `/home/public/HermesVault` o in altre knowledge base condivise;
- accesso limitato a chi gestisce la rotazione delle credenziali.

### Ispezionare la storia originale

```bash
git clone "$AUDIT_DIR/full-history.bundle" /tmp/legacy-pollingweb
cd /tmp/legacy-pollingweb
git log --all --oneline
```

### Verificare che la storia da pubblicare sia pulita

```bash
cd <repo>
PATTERNS="$(paste -sd'|' "$AUDIT_DIR/secret-patterns.txt")"

# a) nessun file tracciato contiene i valori rimossi
git grep -nE "$PATTERNS" -- .

# b) nessun blob di NESSUN commit raggiungibile contiene i valori rimossi
git rev-list --objects --all \
  | awk '{print $1}' \
  | git cat-file --batch-check='%(objectname) %(objecttype)' \
  | awk '$2=="blob"{print $1}' \
  | git cat-file --batch \
  | grep -aoE "$PATTERNS" | sort -u

# c) nessun oggetto irraggiungibile residuo nel repository locale
git count-objects -v          # count=0, garbage=0, size 0
```

---

## 2. Riscrittura della storia (task T0.10)

La storia è stata ricostruita perché conteneva, nel commit radice, credenziali di
produzione e dati di terzi. La repository remota è **privata** (verificato il 2026-10-16),
quindi i valori non sono stati esposti al pubblico: la riscrittura è necessaria per poter
pubblicare senza esporli. Resta comunque obbligatoria la rotazione **prima** della
pubblicazione, per la debolezza intrinseca delle credenziali e perché non è verificabile
l'assenza di copie dal 2023: vedi `docs/SECURITY-FINDINGS.md` §5.

Procedura eseguita e da ripetere se necessario:

```bash
# 0. pausa dei processi automatici che committano (loop schedulati)

# 1. backup verificabile FUORI dal repository
AUDIT_DIR=/root/backups/PollingWeb-<timestamp>
mkdir -p "$AUDIT_DIR" && chmod 700 "$AUDIT_DIR"
git bundle create "$AUDIT_DIR/full-history.bundle" --all
git bundle verify "$AUDIT_DIR/full-history.bundle"

# 2. storia nuova: un commit orfano con il solo contenuto sanificato
git checkout --orphan main
git add -A
git commit -m "chore: pubblica PollingWeb v1.0.0 con storia ripulita"

# 3. rimuovere i riferimenti alla vecchia storia
git branch -D master
git tag -l | xargs -r git tag -d          # nessun tag deve puntare alla vecchia storia
git reflog expire --expire=now --all
git gc --prune=now --aggressive

# 4. verificare (vedi §1)
git rev-list --all --count                # atteso: numero di commit della nuova storia
git count-objects -v
```

### Pubblicazione

Il force-push è l'unico modo per sostituire la storia sul remoto. Va fatto **dopo** aver
verificato il backup e **dopo** aver avviato la rotazione delle credenziali.

```bash
git remote set-url origin https://github.com/<owner>/<repo>.git
git push --force origin main
git push origin --delete master           # rimuove il ramo con la storia vecchia
```

Azioni da completare sul repository remoto:

- abilitare **secret scanning** e **push protection**;
- abilitare l'alerting delle dipendenze;
- verificare che non esistano **fork** con la storia vecchia;
- **verificare che gli oggetti orfani non siano più recuperabili** (vedi sotto).

### Verifica post-push degli oggetti orfani (obbligatoria)

Eseguire il push **non** rende subito irraggiungibili gli oggetti della storia precedente:
GitHub non compatta immediatamente e il commit vecchio resta recuperabile per SHA.

```bash
# sostituire <sha-vecchio> con il commit radice della storia precedente
git fetch origin <sha-vecchio>
# se il fetch riesce, gli oggetti sono ANCORA leggibili dal remoto
```

Verificato il 2026-10-16 sul repo di riferimento: il fetch del commit radice **riusciva**
ancora, a ref rimossi. Conseguenze operative:

1. **la rotazione dei segreti è l'unica mitigazione garantita**: va fatta a prescindere;
2. per la rimozione effettiva chiedere a **GitHub Support** la compattazione della
   repository, indicando gli SHA da eliminare;
3. in alternativa, **cancellare e ricreare** la repository e ripubblicare la storia pulita.

⚠️ Dopo una verifica con `git fetch <sha>`, **ripulire il repository locale**:
l'operazione scarica gli oggetti vecchi e li rende presenti in `.git`.

```bash
rm -f .git/FETCH_HEAD
git reflog expire --expire=now --all
git gc --prune=now
```

⚠️ La riscrittura non cancella le copie già effettuate da chi aveva accesso (clone locali,
backup, macchine di sviluppo). È il motivo per cui la rotazione delle credenziali è
obbligatoria **prima** della pubblicazione, indipendentemente dalla pulizia della storia.

⚠️ Su GitHub, dopo il force-push, gli oggetti orfani possono restare raggiungibili per SHA
per un periodo indefinito. Se serve garanzia, richiedere la rimozione a GitHub Support.

---

## 3. Checklist di pubblicazione (task R6)

Da completare prima di ogni push su un repository pubblico.

### Segreti e dati

- [ ] `git grep -nE "$PATTERNS"` → 0 risultati (pattern dal record di audit);
- [ ] scansione blob su tutti i commit → 0 risultati;
- [ ] `gitleaks detect --no-git` e `gitleaks detect` (con storia) → 0 finding;
- [ ] nessun file in `samples/` contiene dati reali;
- [ ] nessuna nuova dipendenza copyleft.

### Codice

- [ ] `dotnet build PollingWeb.sln -c Release` senza errori su Linux e Windows;
- [ ] `dotnet test` verde, inclusi i test di integrazione con database in container;
- [ ] `bash scripts/verify.sh` (o `scripts/verify.ps1`) esce con 0;
- [ ] nessun `TODO` che nasconda un problema di sicurezza.

### Documentazione

- [ ] `README.md` e `README.it.md` aggiornati e con quickstart verificato su clone pulito;
- [ ] `docs/` completo (`INSTALL`, `ARCHITECTURE`, `CONFIGURATION`, `DATABASE`, `OPERATIONS`,
      `API`, `TROUBLESHOOTING`, `TESTING`, `LEGACY`, `LICENSING`);
- [ ] `CHANGELOG.md` aggiornato con la versione in uscita;
- [ ] `NOTICE` allineato alle dipendenze effettivamente distribuite.

### Artefatti

- [ ] pacchetti di release generati con checksum SHA-256;
- [ ] il pacchetto non contiene file di configurazione con credenziali, solo `.example`;
- [ ] il pacchetto non contiene `bin/`, `obj/`, `.vs/`.

### Documenti di lavoro del loop

`PLAN.md`, `PROMPT.md` e `docs/LOOP_LOG.md` sono artefatti di gestione del lavoro, non
documentazione del prodotto. Prima di una pubblicazione definitiva scegliere **una** delle
due opzioni e annotare la scelta nel `CHANGELOG`:

1. **Non distribuirli**: spostarli in una directory ignorata da Git (es. `docs/internal/`,
   aggiunta a `.gitignore`) e rimuoverli dall'albero pubblicato; oppure
2. **Distribuirli redatti**: sostituire ogni riferimento a host, IP, login, password e
   marchi con placeholder opachi, verificando con il pattern del record di audit.

`docs/SECURITY-FINDINGS.md` è un caso a parte: è utile pubblicarlo (descrive i rischi e il
piano di rotazione) ma deve restare **redatto**. È già in questo stato.

---

## 4. Numerazione delle versioni

Schema [Semantic Versioning](https://semver.org/lang/it/):

- `MAJOR` — rottura del contratto con SAP, delle firme delle stored procedure o delle API;
- `MINOR` — nuove funzionalità retrocompatibili (nuovo processo di polling, nuovo endpoint);
- `PATCH` — correzioni retrocompatibili.

Il numero di versione vive in `Directory.Build.props` (`<Version>`), non nei singoli
progetti.

Il `CHANGELOG.md` segue il formato [Keep a Changelog](https://keepachangelog.com/it/1.1.0/).
