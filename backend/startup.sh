#!/bin/sh

# Esperar o PostgreSQL estar disponível
/wait-for-postgres.sh

# Aplicar as migrações
echo "Aplicando migrações do banco de dados..."
dotnet ef database update --no-build

# Iniciar a aplicação
echo "Iniciando a aplicação..."
exec dotnet Api.dll
