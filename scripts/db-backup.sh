#!/usr/bin/env bash
# Back up the ERP database to a custom-format dump that db-restore.sh can load.
# Usage: ./db-backup.sh [output-file]
# Connection settings default to appsettings.json; override with PGHOST, PGPORT,
# PGDATABASE, PGUSER, PGPASSWORD.
set -euo pipefail

# export PATH="$PATH:<postgres-bin-path>"
export PGHOST="${PGHOST:-localhost}"
export PGPORT="${PGPORT:-5432}"
export PGDATABASE="${PGDATABASE:-ERP}"
export PGUSER="${PGUSER:-sa}"
export PGPASSWORD="${PGPASSWORD:-pass}"

out="${1:-${PGDATABASE}-$(date +%Y%m%d-%H%M%S).backup}"

echo "Backing up '$PGDATABASE' on $PGHOST:$PGPORT ..."
pg_dump --format=custom --no-owner --no-privileges --file="$out"

echo "Done: $out"
