#!/bin/sh

# Wait for PostgreSQL to be ready
/wait-for-postgres.sh

# Apply database migrations
dotnet ef database update --project /src/src/Api/Api.csproj --context AppDbContext

# Start the application
dotnet Api.dll
