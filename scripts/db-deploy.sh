#!/usr/bin/env bash
# =============================================================================
# db-deploy.sh — applica lo schema versionato a un'istanza SQL Server
# =============================================================================
# Fonte di verità: la cartella `sql/`. Vedi docs/adr/0001-sql-schema-source-of-truth.md
#
# Modalità:
#   recreate (default)  elimina e ricrea i database applicativi, poi applica lo schema.
#                       Deterministico e idempotente: due esecuzioni danno lo stesso
#                       risultato. DISTRUTTIVO: solo sviluppo e test.
#   apply               applica lo schema ai database esistenti senza eliminarli.
#                       Non è idempotente per gli oggetti già presenti: serve per
#                       inizializzare un'istanza nuova. Le migrazioni incrementali
#                       sono un problema separato (vedi l'ADR).
#
# Passi eseguiti:
#   1. creazione dei database (Polling, eProcurement, Quartz)
#   2. schema applicativo -> Polling
#   3. schema acquisti -> eProcurement
#   4. schema del job store -> Quartz
#   5. login, utente per database e permessi a privilegi minimi
#   6. autotest dei permessi come utente applicativo (fallisce se qualcosa non torna)
#   7. dati sintetici di samples/ (facoltativo)
#
# Uso:
#   POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh
#   ./scripts/db-deploy.sh --mode apply
#   ./scripts/db-deploy.sh --with-seed        # aggiunge samples/seed/seed.sql
#   POLLING_APP_$1=[REDACTED] ./scripts/db-deploy.sh   # crea e verifica l'utente applicativo
#
# Variabili d'ambiente:
#   POLLING_DB_SERVER     host,porta          (default: localhost,14330)
#   POLLING_DB_USER       utente              (default: sa)
#   POLLING_DB_PASSWORD   password            (obbligatoria)
#   POLLING_APP_LOGIN     login applicativo   (default: PollingApp)
#   POLLING_APP_PASSWORD  password applicativa: se assente, i passi 5 e 6 sono saltati
#                         e NON viene inventata una password
#   POLLING_SQLCMD        comando sqlcmd da usare (se assente, viene dedotto)
#
# L'autotest del passo 6 verifica sia i permessi concessi sia quelli negati: se
# l'utente applicativo riuscisse a eseguire un DDL, il deploy fallirebbe. Un test
# negativo che passa e' un difetto, non un successo.
# =============================================================================
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SQL_DIR="$ROOT/sql"
SAMPLES_DIR="$ROOT/samples"

MODE="recreate"
WITH_SEED="no"

while [ $# -gt 0 ]; do
    case "$1" in
        --mode)      MODE="${2:?--mode richiede un valore}"; shift 2 ;;
        --mode=*)    MODE="${1#*=}"; shift ;;
        --with-seed) WITH_SEED="yes"; shift ;;
        -h|--help)   sed -n '2,30p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'; exit 0 ;;
        *)           echo "opzione sconosciuta: $1" >&2; exit 2 ;;
    esac
done

case "$MODE" in
    recreate|apply) ;;
    *) echo "modalità non valida: $MODE (attese: recreate, apply)" >&2; exit 2 ;;
esac

SERVER="${POLLING_DB_SERVER:-localhost,14330}"
DB_USER="${POLLING_DB_USER:-sa}"
DB_PASSWORD="${POLLING_DB_PASSWORD:-}"
APP_LOGIN="${POLLING_APP_LOGIN:-PollingApp}"
APP_PASSWORD="${POLLING_APP_PASSWORD:-}"

if [ -z "$DB_PASSWORD" ]; then
    echo "ERRORE: POLLING_DB_PASSWORD non impostata." >&2
    echo "       In sviluppo: POLLING_DB_$1=[REDACTED]"\${POLLING_SA_PASSWORD:-DevOnly!Passw0rd2026}\" ./scripts/db-deploy.sh" >&2
    exit 2
fi

# ---------------------------------------------------------------- sqlcmd
# Deduzione del comando: sqlcmd locale se presente, altrimenti quello nel
# container di docker compose (funziona su Linux, macOS e Windows con Docker).
if [ -n "${POLLING_SQLCMD:-}" ]; then
    SQLCMD="$POLLING_SQLCMD"
elif command -v sqlcmd >/dev/null 2>&1; then
    SQLCMD="sqlcmd"
elif command -v /opt/mssql-tools18/bin/sqlcmd >/dev/null 2>&1; then
    SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif docker compose -f "$ROOT/docker-compose.yml" ps --status running --services 2>/dev/null | grep -qx mssql; then
    # sqlcmd eseguito dentro del container; i file gli vengono passati su stdin.
    SQLCMD="__DOCKER__"
else
    echo "ERRORE: nessun sqlcmd disponibile." >&2
    echo "        Installare mssql-tools18, oppure avviare il container: docker compose up -d" >&2
    echo "        e rieseguire. In alternativa impostare POLLING_SQLCMD." >&2
    exit 3
fi

# I file .sql del repository hanno il BOM UTF-8 (eredità di SSDT/Visual Studio).
# `sqlcmd -i` lo gestisce, ma su stdin il BOM viene interpretato come parte della
# prima istruzione e SQL Server risponde "Incorrect syntax near ''". Questa
# funzione emette il file senza BOM iniziale, senza modificare il file su disco.
emit_sql() {
    local f="$1"
    local magic
    magic="$(head -c 3 "$f" | od -An -tx1 | tr -d ' \n')"
    if [ "$magic" = "efbbbf" ]; then
        tail -c +4 "$f"
    else
        cat "$f"
    fi
}

# Esecutore unico: le credenziali sono parametri, così lo stesso codice serve sia il
# deploy (utente con privilegi elevati) sia l'autotest (utente applicativo).
if [ "$SQLCMD" = "__DOCKER__" ]; then
    _sqlcmd() {
        local user="$1" pw="$2" db="$3"; shift 3
        docker compose -f "$ROOT/docker-compose.yml" exec -T mssql \
            /opt/mssql-tools18/bin/sqlcmd -S localhost -U "$user" -P "$pw" -C -b -d "$db" "$@"
    }
    echo "sqlcmd: dentro il container 'mssql' (docker compose exec)"
else
    _sqlcmd() {
        local user="$1" pw="$2" db="$3"; shift 3
        "$SQLCMD" -S "$SERVER" -U "$user" -P "$pw" -C -b -d "$db" "$@"
    }
    echo "sqlcmd: $SQLCMD (server $SERVER)"
fi

# Tutto passa da stdin con il BOM rimosso: un solo percorso per entrambe le modalità.
run_sql()     { local db="$1" file="$2"; shift 2; emit_sql "$file"      | _sqlcmd "$DB_USER"  "$DB_PASSWORD"  "$db" "$@"; }
run_query()   { printf '%s\n' "$2" | _sqlcmd "$DB_USER"  "$DB_PASSWORD"  "$1"; }
run_sql_as()  { local db="$1" file="$2"; shift 2; emit_sql "$file"      | _sqlcmd "$APP_LOGIN" "$APP_PASSWORD" "$db" "$@"; }
run_query_as(){ printf '%s\n' "$2" | _sqlcmd "$APP_LOGIN" "$APP_PASSWORD" "$1"; }

step() { printf '\n=== %s\n' "$*"; }
ok()   { printf '    ok   %s\n' "$*"; }
fail() { printf '    FALLITO  %s\n' "$*"; FAILURES=$((FAILURES + 1)); }
FAILURES=0

# ---------------------------------------------------------------- autotest dei permessi
# Verifica che l'utente applicativo possa fare esattamente ciò che serve e nulla di più.
# Girano come utente applicativo (non con EXECUTE AS): è l'unica prova che autentica
# davvero quelle credenziali e attraversa anche i confini fra database.
expect_ok()   { local desc="$1" db="$2" sql="$3"; if run_query_as "$db" "$sql" >/dev/null 2>&1; then ok "$desc"; else fail "$desc  <- permesso mancante"; fi; }
expect_deny() { local desc="$1" db="$2" sql="$3"; if run_query_as "$db" "$sql" >/dev/null 2>&1; then fail "$desc  <- permesso NON negato"; else ok "$desc"; fi; }

# ------------------------------------------------------- 1. database
DATABASES="Polling eProcurement Quartz"

step "1/7 database ($MODE)"
for db in $DATABASES; do
    if [ "$MODE" = "recreate" ]; then
        run_query master "IF DB_ID(N'$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END; CREATE DATABASE [$db];" >/dev/null
        ok "$db (ricreato)"
    else
        run_query master "IF DB_ID(N'$db') IS NULL CREATE DATABASE [$db];" >/dev/null
        ok "$db (creato se assente)"
    fi
done

# ------------------------------------------------------- 2. schema applicativo
# Ordine: tabelle -> funzioni -> viste -> stored procedure.
# Le stored procedure non validano gli oggetti referenziati alla creazione
# (name resolution differita), quindi l'ordine conta solo per tabelle/viste.
step "2/7 schema applicativo -> Polling"
apply_dir() {
    local db="$1" dir="$2"
    [ -d "$dir" ] || return 0
    find "$dir" -maxdepth 1 -name '*.sql' -print0 | sort -z | while IFS= read -r -d '' f; do
        run_sql "$db" "$f" >/dev/null
        ok "$(basename "$f")"
    done
}
for sub in dbo/Tables dbo/Functions dbo/Views "dbo/Stored Procedures"; do
    apply_dir "Polling" "$SQL_DIR/PollingDB/$sub"
done

# ------------------------------------------------------- 3. schema eProcurement
# Ordine: tabelle -> funzioni -> viste. Lo schema e' ricostruito dalla sola DDL
# dello snapshot storico (task T0.12): nessun dato reale e' versionato.
step "3/7 schema acquisti -> eProcurement"
for sub in dbo/Tables dbo/Functions dbo/Views; do
    apply_dir "eProcurement" "$SQL_DIR/eProcurement/$sub"
done

# ------------------------------------------------------- 4. schema Quartz
step "4/7 schema del job store -> Quartz"
if [ -f "$SQL_DIR/Quartz/quartz-schema.sql" ]; then
    run_sql Quartz "$SQL_DIR/Quartz/quartz-schema.sql" >/dev/null
    ok "quartz-schema.sql"
else
    echo "    ATTENZIONE: sql/Quartz/quartz-schema.sql assente" >&2
fi

# ------------------------------------------------------- 5. sicurezza
step "5/7 login, utente e permessi"
if [ -z "$APP_PASSWORD" ]; then
    echo "    saltato: POLLING_APP_PASSWORD non impostata (nessuna credenziale inventata)"
    echo "    per creare il login applicativo e verificarne i permessi:"
    echo "      POLLING_APP_$1=[REDACTED] ./scripts/db-deploy.sh --mode apply"
else
    # Il login è a livello server: va creato una volta sola.
    run_sql master "$SQL_DIR/PollingDB/Security/01-create-app-login.sql" \
        -v "PollingAppLogin=$APP_LOGIN" -v "PollingApp$1=[REDACTED]" >/dev/null
    ok "login $APP_LOGIN"
    # Utente per database + permessi a privilegi minimi specifici di quel database.
    for db in $DATABASES; do
        run_sql "$db" "$SQL_DIR/PollingDB/Security/02-create-app-user.sql" \
            -v "PollingAppLogin=$APP_LOGIN" >/dev/null
        case "$db" in
            Polling)      perms="$SQL_DIR/PollingDB/Security/Permissions.sql" ;;
            eProcurement) perms="$SQL_DIR/eProcurement/Security/Permissions.sql" ;;
            Quartz)       perms="$SQL_DIR/Quartz/Security/Permissions.sql" ;;
        esac
        if [ -f "$perms" ]; then
            run_sql "$db" "$perms" -v "PollingAppLogin=$APP_LOGIN" >/dev/null
            ok "utente e permessi su $db ($(basename "$(dirname "$perms")")/$(basename "$perms"))"
        else
            fail "permessi mancanti per $db: atteso $perms"
        fi
    done
fi

# ------------------------------------------------------- 6. autotest dei permessi
step "6/7 autotest dei permessi come utente applicativo"
if [ -z "$APP_PASSWORD" ]; then
    echo "    saltato: POLLING_APP_PASSWORD non impostata"
else
    # Positivi: ciò che l'applicazione deve poter fare.
    expect_ok   "lettura tabella applicativa"            Polling       "SELECT TOP 1 1 FROM dbo.TaskService;"
    expect_ok   "scrittura tabella applicativa"           Polling       "BEGIN TRAN; INSERT INTO dbo.Tool (Name) VALUES (N'perm-test'); DELETE FROM dbo.Tool WHERE Name = N'perm-test'; COMMIT;"
    expect_ok   "lettura vista"                           Polling       "SELECT TOP 1 1 FROM dbo.ViewTaskService;"
    # Per la funzione si usa DATEFROMPARTS e NON un literal '2024-03-15': il login
    # applicativo viene creato con DEFAULT_LANGUAGE = [Italiano], quindi un literal
    # ISO verrebbe interpretato come gg/mm/aaaa e fallirebbe con Msg 242. Il test
    # deve verificare il PERMESSO, non la conversione implicita di data.
    expect_ok   "invocazione funzione scalare"            Polling       "SELECT dbo.Datini(DATEFROMPARTS(2024, 3, 15));"
    expect_ok   "esecuzione stored procedure"             Polling       "DECLARE @o nvarchar(30); EXEC dbo.AzzeraTargetSC00G100 @pi_istituto = 1, @po_valoreuscita = @o OUTPUT;"
    expect_ok   "scrittura su eProcurement"              eProcurement  "BEGIN TRAN; INSERT INTO dbo.Log_ODA_SAP (Istituto, Data_Put) VALUES (1, GETDATE()); ROLLBACK;"
    expect_ok   "lettura anagrafica fornitori"           eProcurement  "SELECT TOP 1 1 FROM dbo.DimFornitori;"
    expect_ok   "accesso al job store Quartz"            Quartz        "SELECT TOP 1 1 FROM dbo.QRTZ_LOCKS;"
    # Negativi: ciò che NON deve poter fare. Un test negativo che passa è un difetto.
    expect_deny "DDL negata (CREATE TABLE)"               Polling       "CREATE TABLE dbo.__pollingweb_should_fail (x int);"
    expect_deny "DDL negata su eProcurement"             eProcurement  "CREATE TABLE dbo.__pollingweb_should_fail (x int);"
    expect_deny "DDL negata su Quartz"                   Quartz        "CREATE TABLE dbo.__pollingweb_should_fail (x int);"
    expect_deny "scrittura su database di sistema"       master        "BEGIN TRAN; INSERT INTO dbo.spt_values (name) VALUES (N'x'); ROLLBACK;"
fi

# ------------------------------------------------------- 7. dati sintetici
step "7/7 dati di esempio"
if [ "$WITH_SEED" = "yes" ]; then
    run_sql Polling "$SAMPLES_DIR/seed/seed.sql" >/dev/null
    ok "samples/seed/seed.sql (dati SINTETICI, mai dati reali)"
else
    echo "    saltato: usare --with-seed per inserire i dati sintetici di samples/"
fi

printf '\nDeploy completato. Modalità: %s\n' "$MODE"
if [ "$FAILURES" -gt 0 ]; then
    printf 'Autotest dei permessi: %d controlli FALLITI.\n' "$FAILURES" >&2
    exit 1
fi
printf 'Autotest dei permessi: superato.\n'
printf 'Verifica rapida:\n'
printf "  POLLING_DB_$1=[REDACTED] %s\n" "$0 --mode apply"
