#!/usr/bin/env bash
# Restore a db-backup.sh dump into a fresh, empty ERP database.
# Usage: ./db-restore.sh [-y] <file.backup>
#   -y  skip the confirmation prompt
# Connection settings default to appsettings.json; override with PGHOST, PGPORT,
# PGDATABASE, PGUSER, PGPASSWORD.
set -euo pipefail

# export PATH="$PATH:<postgres-bin-path>"
export PGHOST="${PGHOST:-localhost}"
export PGPORT="${PGPORT:-5432}"
export PGDATABASE="${PGDATABASE:-ERP}"
export PGUSER="${PGUSER:-sa}"
export PGPASSWORD="${PGPASSWORD:-pass}"

assume_yes=false
if [[ "${1:-}" == "-y" ]]; then
    assume_yes=true
    shift
fi

file="${1:-}"
if [[ -z "$file" ]]; then
    echo "Usage: $0 [-y] <file.backup>" >&2
    exit 1
fi
if [[ ! -f "$file" ]]; then
    echo "File not found: $file" >&2
    exit 1
fi

if [[ "$assume_yes" != true ]]; then
    echo "This DROPS database '$PGDATABASE' on $PGHOST:$PGPORT and replaces it with $file."
    read -r -p "Continue? [y/N] " answer
    if [[ ! "$answer" =~ ^[Yy]$ ]]; then
        echo "Aborted -- nothing was changed."
        exit 1
    fi
fi

# Restoring into an existing schema leaves identity sequences behind the data,
# so always start from an empty database. --force closes open connections.
echo "Recreating database '$PGDATABASE' ..."
dropdb --maintenance-db=postgres --if-exists --force "$PGDATABASE"
createdb --maintenance-db=postgres "$PGDATABASE"

echo "Restoring $file ..."
pg_restore --no-owner --no-privileges --exit-on-error --dbname="$PGDATABASE" "$file"

# Safety net: move every identity/serial sequence past its table's MAX(Id).
echo "Resetting identity sequences ..."
psql --quiet -v ON_ERROR_STOP=1 --dbname="$PGDATABASE" <<'SQL'
DO $$
DECLARE r record;
BEGIN
    FOR r IN
        SELECT c.table_name, c.column_name,
               pg_get_serial_sequence(format('%I.%I', c.table_schema, c.table_name), c.column_name) AS seq
        FROM information_schema.columns c
        WHERE c.table_schema = 'public'
          AND pg_get_serial_sequence(format('%I.%I', c.table_schema, c.table_name), c.column_name) IS NOT NULL
    LOOP
        EXECUTE format(
            'SELECT setval(%L, COALESCE((SELECT MAX(%I) FROM %I), 1), EXISTS (SELECT 1 FROM %I))',
            r.seq, r.column_name, r.table_name, r.table_name);
    END LOOP;
END $$;
SQL

echo "Done. Start the backend (don't run 'dotnet ef database update' first)."
