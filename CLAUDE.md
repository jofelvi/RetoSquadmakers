# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 Web API project (`retoSquadmakers`) built with ASP.NET Core. It's a minimal API that currently provides a weather forecast endpoint and includes Entity Framework Core with PostgreSQL support.

## Architecture

- **Framework**: ASP.NET Core 8.0 with minimal APIs
- **Database**: PostgreSQL with Entity Framework Core 9.0.7
- **API Documentation**: Swagger/OpenAPI integration
- **Environment**: Configured for Development and Production environments

The main application logic is in `Program.cs` using the minimal hosting model. The project includes:
- Swagger UI available at `/swagger` endpoint
- Single weather forecast API endpoint at `/weatherforecast`
- Entity Framework tooling for migrations and database operations
- HTTPS redirection and development-specific middleware

## Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the application (Development mode)
dotnet run

# Run with specific profile
dotnet run --launch-profile https

# RECOMMENDED: Run in background with logs
nohup dotnet run > /tmp/dotnet-app.log 2>&1 &

# Check if application is running
ps aux | grep dotnet | grep -v grep

# Check if port is listening
ss -tlnp | grep :5062

# Stop background application
pkill -f "dotnet run"
```

### Database Operations
```bash
# Add new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Testing and Quality
```bash
# Run tests (when test projects are added)
dotnet test

# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean
```

## Configuration

- **Development URL**: http://localhost:5062 (HTTP) / https://localhost:7078 (HTTPS)
- **Swagger UI**: Available at `/swagger` in development mode
- **Environment Variables**: Set via `launchSettings.json` or system environment
- **App Settings**: Configured in `appsettings.json` and `appsettings.Development.json`

## HTTP Testing

Use the `retoSquadmakers.http` file with REST Client extensions to test the API endpoints. The base URL is configured as `http://localhost:5062`.

## Database Setup

The project is configured for PostgreSQL. Ensure you have:
1. PostgreSQL server running
2. Connection string configured in appsettings
3. Run `dotnet ef database update` after any schema changes

## Troubleshooting

### Application Not Starting
**Problem**: Application seems to start but URLs don't work
**Causes**:
1. **Foreground vs Background**: `dotnet run` in foreground gets killed easily
2. **Compilation Time**: App needs time to compile before listening
3. **Port Conflicts**: Port 5062 might be in use by another process
4. **HTTPS Issues**: Warning about HTTPS port determination

**Solutions**:
```bash
# 1. Always check if port is free first
ss -tlnp | grep :5062

# 2. Run in background with logging
nohup dotnet run > /tmp/dotnet-app.log 2>&1 &

# 3. Wait for compilation and startup (check logs)
tail -f /tmp/dotnet-app.log

# 4. Verify application is actually listening
curl -s -o /dev/null -w "%{http_code}" http://localhost:5062/weatherforecast

# 5. Clean restart if needed
pkill -f "dotnet run"
dotnet clean
nohup dotnet run > /tmp/dotnet-app.log 2>&1 &
```

**Key Indicators**:
- ✅ Good: `Now listening on: http://localhost:5062`
- ⚠️ Warning: `Failed to determine the https port for redirect`
- ❌ Bad: No process found with `ps aux | grep dotnet`