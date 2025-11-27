# PostgreSQL Migration Guide

This document outlines the migration from SQL Server to PostgreSQL for the Invoice Backend API.

## What Changed

### 1. NuGet Packages
- **Removed**: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.11)
- **Added**: `Npgsql.EntityFrameworkCore.PostgreSQL` (8.0.0)
- **Added**: `System.IdentityModel.Tokens.Jwt` (7.1.2) - for JWT authentication
- **Added**: `Microsoft.IdentityModel.Tokens` (7.1.2) - for JWT authentication

### 2. Connection String
**Before (SQL Server)**:
```
Server=(local);Database=InvoiceGeneratorDb;Trusted_Connection=true;TrustServerCertificate=true;
```

**After (PostgreSQL)**:
```
Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres
```

### 3. Program.cs Configuration
**Before**:
```csharp
options.UseSqlServer(connectionString);
```

**After**:
```csharp
options.UseNpgsql(connectionString);
```

## Setup Instructions

### 1. Install PostgreSQL

#### macOS (using Homebrew)
```bash
brew install postgresql@15
brew services start postgresql@15
```

#### Ubuntu/Debian
```bash
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib
sudo systemctl start postgresql
```

#### Windows
Download and install from [postgresql.org](https://www.postgresql.org/download/windows/)

### 2. Create Database and User

Connect to PostgreSQL:
```bash
psql -U postgres
```

Create database and user:
```sql
-- Create database
CREATE DATABASE invoice_generator_db;

-- Create user (if not using default postgres user)
-- CREATE USER invoice_user WITH PASSWORD 'secure_password';
-- GRANT ALL PRIVILEGES ON DATABASE invoice_generator_db TO invoice_user;
```

### 3. Update Connection String

Update `appsettings.json` with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=your_password"
  }
}
```

**Important**: For production, use strong passwords and consider using environment variables or secrets management.

### 4. Create Database Migrations

If you have existing migrations from SQL Server, you'll need to create new ones for PostgreSQL:

```bash
# Remove old migrations (if migrating from SQL Server)
dotnet ef migrations remove

# Create initial migration for PostgreSQL
dotnet ef migrations add InitialPostgresqlCreate

# Apply migrations to database
dotnet ef database update
```

If this is a fresh PostgreSQL setup:
```bash
# Create initial migration
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update
```

### 5. Verify Connection

Start the API:
```bash
dotnet run
```

Check the logs for successful database connection:
```
[INFO] Successfully opened connection to database 'postgres' on server 'localhost'.
```

## Connection String Variations

### Local Development (Default)
```
Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres
```

### Production with Environment Variable
```
Host=$PG_HOST;Port=$PG_PORT;Database=$PG_DATABASE;Username=$PG_USER;Password=$PG_PASSWORD
```

### Docker PostgreSQL
```
Host=postgres;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres
```
(When running via Docker Compose, service name is used instead of localhost)

### SSL/TLS Connection
```
Host=your-server.com;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres;SSL Mode=Require
```

## Testing the Connection

You can test the connection using `psql`:

```bash
psql -h localhost -U postgres -d invoice_generator_db
```

Or using the connection string:
```bash
psql "postgresql://postgres:postgres@localhost:5432/invoice_generator_db"
```

## Backup and Restore

### Backup Database
```bash
pg_dump -U postgres -d invoice_generator_db -f backup.sql
```

### Restore Database
```bash
psql -U postgres -d invoice_generator_db -f backup.sql
```

## Common Issues

### Issue: "psql: error: could not translate host name 'localhost' to address"
**Solution**: Use `127.0.0.1` instead of `localhost` in connection string.

### Issue: "FATAL: password authentication failed for user 'postgres'"
**Solution**: Verify the password in your connection string matches the PostgreSQL user password.

### Issue: "database 'invoice_generator_db' does not exist"
**Solution**: Create the database using the SQL commands above.

### Issue: "Npgsql.NpgsqlException: Prepared statement... already exists"
**Solution**: Clear Entity Framework cache:
```bash
dotnet ef database update 0
dotnet ef database update
```

## Troubleshooting

### Check PostgreSQL Service Status
```bash
# macOS
brew services list

# Ubuntu/Debian
sudo systemctl status postgresql

# Windows (Services)
Services > PostgreSQL
```

### View Logs
```bash
# Connection logs appear in dotnet run output
# To enable PostgreSQL query logging:
```

Add to `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres;Log Parameters=true"
  }
}
```

### Reset Database

```bash
psql -U postgres
DROP DATABASE invoice_generator_db;
CREATE DATABASE invoice_generator_db;
```

Then rerun migrations:
```bash
dotnet ef database update
```

## Performance Tuning

### Connection Pooling
The connection string already includes optimal pooling settings through Npgsql. To customize:

```
Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres;Max Pool Size=20;Min Pool Size=5
```

### Query Timeouts
```
Host=localhost;Port=5432;Database=invoice_generator_db;Username=postgres;Password=postgres;Command Timeout=30
```

## Migration from SQL Server Data

If you have existing data in SQL Server:

1. Export data from SQL Server
2. Transform to PostgreSQL-compatible format
3. Import into PostgreSQL

Tools:
- pgAdmin (GUI)
- DBeaver (Multi-database tool)
- Custom scripts

## Next Steps

1. ✅ Updated NuGet packages
2. ✅ Updated connection string
3. ✅ Updated Program.cs configuration
4. 📋 Create/update database migrations
5. 📋 Test all API endpoints
6. 📋 Set up automated backups
7. 📋 Configure production environment variables

## References

- [Npgsql Documentation](https://www.npgsql.org/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core PostgreSQL](https://www.npgsql.org/efcore/index.html)
