#!/bin/bash

# Espera até que o PostgreSQL esteja disponível
until pg_isready -h db -p 5432; do
  echo "Aguardando o PostgreSQL..."
  sleep 2
done

echo "Postgres está pronto - executando comando"

# Aplicar migrações
dotnet ef database update --project /api/Api/Api.csproj

# Executar o comando dotnet run
dotnet run --project /api/Api/Api.csproj
