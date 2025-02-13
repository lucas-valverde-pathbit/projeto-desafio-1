#!/bin/bash
# Aguardar até que o PostgreSQL esteja disponível
until pg_isready -h "$1" -p 5432; do
  echo "Postgres is unavailable - sleeping"
  sleep 1
done

echo "Postgres is up - executing command"
exec "$2" "$3" "$4"
