#!/usr/bin/env bash
# =============================================================================
# test-integration.sh — prepara l'istanza e lancia i test di integrazione
# =============================================================================
# Fa esattamente tre cose, nello stesso ordine che seguirebbe un essere umano:
#   1. avvia il container SQL Server se non è già attivo
#   2. applica lo schema con scripts/db-deploy.sh (fonte unica dello schema)
#   3. esegue i test di integrazione
#
# Uso:
#   POLLING_DB_$1=[REDACTED] POLLING_APP_$1=[REDACTED] \
#       ./scripts/test-integration.sh
#
# Senza POLLING_DB_PASSWORD il deploy non può partire e lo script esce 2.
# Senza POLLING_APP_PASSWORD i test dei permessi si saltano da soli (non falliscono):
# un test che fallisce perché manca una credenziale non segnala un difetto del codice.
#
# I test si saltano da soli anche se SQL Server non risponde: il salto è dichiarato
# nell'output di `dotnet test` e non va confuso con un fallimento.
# =============================================================================
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [ -z "${POLLING_DB_PASSWORD:-}" ]; then
    echo "ERRORE: POLLING_DB_PASSWORD non impostata." >&2
    echo "        In sviluppo: POLLING_DB_$1=[REDACTED]"\${POLLING_SA_PASSWORD:-DevOnly!Passw0rd2026}\"" >&2
    exit 2
fi

export PATH="${PATH:-}"

echo "=== 1/3 container SQL Server"
if docker compose ps --status running --services 2>/dev/null | grep -qx mssql; then
    echo "    già attivo: pollingweb-mssql"
else
    docker compose up -d
    echo "    avviato; attendo che sia pronto"
    for _ in $(seq 1 30); do
        stato="$(docker inspect --format='{{.State.Health.Status}}' pollingweb-mssql 2>/dev/null || echo sconosciuto)"
        [ "$stato" = "healthy" ] && break
        sleep 2
    done
    echo "    stato: $(docker inspect --format='{{.State.Health.Status}}' pollingweb-mssql 2>/dev/null || echo sconosciuto)"
fi

echo
echo "=== 2/3 schema (scripts/db-deploy.sh)"
./scripts/db-deploy.sh --with-seed

# SQL Server container fixture: the same least-privilege application login and the
# Quartz database are used. Quartz 3.x schema migration runs in-place in T0.8 so this
# integration smoke can prove persistent trigger recovery without adding a privileged
# second credential to the test process.
export POLLING_QUARTZ_CONNECTION="${POLLING_QUARTZ_CONNECTION:-Server=${POLLING_DB_SERVER:-localhost,14330};Database=Quartz;User Id=${POLLING_APP_LOGIN:-PollingApp};$1=[REDACTED];Encrypt=False;TrustServerCertificate=True;Connect Timeout=5;}"

echo
echo "=== 3/3 test di integrazione"
dotnet test tests/PollingWeb.IntegrationTests/PollingWeb.IntegrationTests.csproj \
    --logger "console;verbosity=normal"
