#!/bin/sh

# Wait for PostgreSQL to be ready
/wait-for-postgres.sh

# Apply database migrations and seed data
echo "Applying database migrations..."
dotnet Api.dll --migrate

# Start the application
echo "Starting application..."
exec dotnet Api.dll
