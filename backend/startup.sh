#!/bin/sh

# Wait for PostgreSQL to be ready
/wait-for-postgres.sh

# Apply database migrations and start the application
dotnet Api.dll --migrate
