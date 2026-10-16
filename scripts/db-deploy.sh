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
# Uso:
#   POLLING_DB_$1=[REDACTED] ./scripts/db-deploy.sh
#   ./scripts/db-deploy.sh --mode apply
#   ./scripts/db-deploy.sh --with-seed        # aggiunge samples/seed/seed.sql
#
# Variabili d'ambiente:
#   POLLING_DB_SERVER     host,porta          (default: localhost,14330)
#   POLLING_DB_USER       utente              (default: sa)
#   POLLING_DB_PASSWORD   password            (obbligatoria)
#   POLLING_APP_LOGIN     login applicativo   (default: PollingApp; per gli script Security)
#   POLLING_APP_PASSWORD  password applicativa (se assente, gli script Security sono saltati)
#   POLLING_SQLCMD        comando sqlcmd da usare (se assente, viene dedotto)
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

if [ "$SQLCMD" = "__DOCKER__" ]; then
    run_sql() {  # run_sql <database> <file> [extra args...]
        local db="$1" file="$2"; shift 2
        emit_sql "$file" | docker compose -f "$ROOT/docker-compose.yml" exec -T mssql \
            /opt/mssql-tools18/bin/sqlcmd -S localhost -U "$DB_USER" -P "$DB_PASSWORD" -C -b -d "$db" "$@"
    }
    run_query() {  # run_query <database> <sql>
        printf '%s\n' "$2" | docker compose -f "$ROOT/docker-compose.yml" exec -T mssql \
            /opt/mssql-tools18/bin/sqlcmd -S localhost -U "$DB_USER" -P "$DB_PASSWORD" -C -b -d "$1"
    }
    echo "sqlcmd: dentro il container 'mssql' (docker compose exec)"
else
    run_sql() {
        local db="$1" file="$2"; shift 2
        # Con -i il BOM è gestito da sqlcmd: nessuna trasformazione necessaria.
        "$SQLCMD" -S "$SERVER" -U "$DB_USER" -P "$DB_PASSWORD" -C -b -d "$db" "$@" -i "$file"
    }
    run_query() {
        "$SQLCMD" -S "$SERVER" -U "$DB_USER" -P "$DB_PASSWORD" -C -b -d "$1" -Q "$2"
    }
    echo "sqlcmd: $SQLCMD (server $SERVER)"
fi

step() { printf '\n=== %s\n' "$*"; }
ok()   { printf '    ok   %s\n' "$*"; }

# ------------------------------------------------------- 1. database
DATABASES="Polling eProcurement Quartz"

step "1/5 database ($MODE)"
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
step "2/5 schema applicativo -> Polling"
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

# ------------------------------------------------------- 3. schema Quartz
step "3/5 schema del job store -> Quartz"
if [ -f "$SQL_DIR/Quartz/quartz-schema.sql" ]; then
    run_sql Quartz "$SQL_DIR/Quartz/quartz-schema.sql" >/dev/null
    ok "quartz-schema.sql"
else
    echo "    ATTENZIONE: sql/Quartz/quartz-schema.sql assente" >&2
fi

# ------------------------------------------------------- 4. sicurezza
step "4/5 login, utente e permessi"
if [ -z "$APP_PASSWORD" ]; then
    echo "    saltato: POLLING_APP_PASSWORD non impostata (nessuna credenziale inventata)"
    echo "    per creare il login applicativo:"
    echo "      POLLING_APP_$1=[REDACTED] ./scripts/db-deploy.sh --mode apply"
else
    # Il login è a livello server: va creato una volta sola.
    run_sql master "$SQL_DIR/PollingDB/Security/01-create-app-login.sql" \
        -v "PollingAppLogin=$APP_LOGIN" -v "PollingApp$1=[REDACTED]" >/dev/null
    ok "login $APP_LOGIN"
    for db in $DATABASES; do
        run_sql "$db" "$SQL_DIR/PollingDB/Security/02-create-app-user.sql" \
            -v "PollingAppLogin=$APP_LOGIN" >/dev/null
        run_sql "$db" "$SQL_DIR/PollingDB/Security/Permissions.sql" \
            -v "PollingAppLogin=$APP_LOGIN" >/dev/null
        ok "utente e permessi su $db"
    done
fi

# ------------------------------------------------------- 5. dati sintetici
step "5/5 dati di esempio"
if [ "$WITH_SEED" = "yes" ]; then
    run_sql Polling "$SAMPLES_DIR/seed/seed.sql" >/dev/null
    ok "samples/seed/seed.sql (dati SINTETICI, mai dati reali)"
else
    echo "    saltato: usare --with-seed per inserire i dati sintetici di samples/"
fi

printf '\nDeploy completato. Modalità: %s\n' "$MODE"
printf 'Verifica rapida:\n'
printf "  POLLING_DB_$1=[REDACTED] %s\n" "$0 --mode apply"
