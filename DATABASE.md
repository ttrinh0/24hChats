# Local SQL Server in Docker

This project runs SQL Server using `compose.yaml`. The ASP.NET Core app runs on Windows and connects to the database in Docker.

| Setting | Value |
| --- | --- |
| Compose service | `db` |
| Container name (default for this project folder) | `24hchats-db-1` |
| Database | `Chats24h` |
| Login | `sa` |
| Windows connection address | `tcp:127.0.0.1,1433` |
| Address inside the database container | `localhost` |
| Persistent storage | Named volume `chat_sql_data` (normally `24hchats_chat_sql_data`) |

Use the password configured in `MSSQL_SA_PASSWORD` in `compose.yaml`. The app connection string must use the same password. SQL Server Developer edition is for development and testing, not production.

## 1. Open the project terminal

Start Docker Desktop with Linux containers enabled. Run these commands in **Windows PowerShell**:

```powershell
cd C:\Users\1stas\projects\24hChats
docker version
```

## 2. Create and start the database container

```powershell
docker compose up -d db
```

This downloads the image if necessary, creates the container and named volume, and starts the database in the background.

Check status and follow startup logs:

```powershell
docker compose ps
docker compose logs -f --tail 60 db
```

Look for `SQL Server is now ready for client connections` and confirm there are no subsequent setup errors or repeated restarts. Press **Ctrl+C** to stop following logs; the container keeps running.

## 3. Configure the app connection

The `ConnectionStrings:DefaultConnection` setting in `appsettings.Development.json` should have this structure. Replace `YOUR_PASSWORD` with the password from Compose:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=tcp:127.0.0.1,1433;Database=Chats24h;User Id=sa;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=True"
}
```

This is a JSON fragment to place inside the existing settings object. Keep the other settings.

Use `tcp:127.0.0.1,1433` rather than `localhost,1433`: the explicit IPv4 address worked in this project's connection checks, while `localhost` failed. Docker publishes the port on `127.0.0.1`.

If an old User Secrets connection string overrides this setting, remove that specific override:

```powershell
dotnet user-secrets remove "ConnectionStrings:DefaultConnection"
```

Hardcoded passwords and `TrustServerCertificate=True` are local development settings. Do not reuse these credentials for production or commit real credentials.

## 4. Create the database tables and run the app

After SQL Server is ready, run from the Windows project terminal:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update
dotnet watch
```

`database update` creates the database if needed and applies existing migrations. No new migration is required just for switching from LocalDB to Docker. LocalDB records are not automatically transferred.

To inspect migration status:

```powershell
dotnet ef migrations list
```

Stop `dotnet watch` with **Ctrl+C** before restarting the app after configuration changes.

## 5. Connect from Windows PowerShell

From the project folder:

```powershell
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C
```

Enter the database password at the prompt. It may not display any characters while you type or paste.

The command runs `sqlcmd` inside the container:

- `-S localhost`: connects to SQL Server inside that container.
- `-U sa`: selects the SQL administrator login.
- `-C`: trusts the local server certificate.

## 6. Connect from Docker Desktop's container terminal

Open **Containers → 24hchats-db-1 → Exec** (the tab name may vary by version). In that Linux terminal, run:

```bash
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C
```

Try **Ctrl+Shift+V**, **Shift+Insert**, or the terminal's paste menu to paste. Password input may remain invisible; press Enter after pasting.

Do not run `dotnet ef` in this database container: your project and .NET development tools run on Windows.

## 7. Useful SQL commands

Once connected, `sqlcmd` displays a `1>` prompt. Do not type the prompt itself. Put `GO` on its own line to execute each SQL batch.

List databases:

```sql
SELECT name FROM sys.databases;
GO
```

Select the app database:

```sql
USE Chats24h;
GO
```

List tables:

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
GO
```

Show rooms, newest first:

```sql
SELECT Id, Name, CreatedAt, ExpiresAt
FROM dbo.ChatRooms
ORDER BY CreatedAt DESC;
GO
```

Show only active rooms:

```sql
SELECT Id, Name, ExpiresAt
FROM dbo.ChatRooms
WHERE ExpiresAt > SYSUTCDATETIME()
ORDER BY CreatedAt DESC;
GO
```

Inspect applied migrations:

```sql
SELECT MigrationId, ProductVersion
FROM dbo.__EFMigrationsHistory
ORDER BY MigrationId;
GO
```

Exit `sqlcmd`:

```text
EXIT
```

Rooms are stored in the database. Chat messages currently travel through SignalR without being stored, so there is no message-history table. The app's cleanup service deletes expired rooms while the app is running; starting only the database does not run that worker.

## 8. Everyday container commands

Run these in Windows PowerShell from the project directory.

Stop the database without deleting its data:

```powershell
docker compose stop db
```

Start the existing stopped container:

```powershell
docker compose start db
```

Restart the existing container:

```powershell
docker compose restart db
```

Apply changes to Compose configuration by recreating the container while retaining its volume:

```powershell
docker compose up -d --force-recreate db
```

Remove the project's containers and network while retaining the named database volume:

```powershell
docker compose down
```

Recreate them later with `docker compose up -d db`.

**Do not add `-v` to `docker compose down` unless you deliberately want to delete the named volumes and their database data.**

## 9. Troubleshooting

### Container repeatedly restarts

```powershell
docker compose logs --tail 100 db
```

The earlier restart loop in this project was caused by a password that was too short. SQL Server requires at least eight characters and characters from at least three of these categories: uppercase letters, lowercase letters, digits, and symbols.

If initial setup failed password validation, correct the Compose password, match it in the app connection string, and recreate the container:

```powershell
docker compose up -d --force-recreate db
```

For an already initialized database, changing `MSSQL_SA_PASSWORD` in Compose does not reset the password stored in the existing database volume.

### App connection times out

```powershell
docker compose ps
docker compose logs --tail 60 db
Test-NetConnection 127.0.0.1 -Port 1433
```

Check that the container stays running, the port mapping is present, and the app uses `Server=tcp:127.0.0.1,1433`. A successful TCP port test alone does not verify SQL login or database readiness.

### Login failed for user 'sa'

Check that the app password matches the password used to initialize the database. Check for an older User Secrets or environment-variable override.

### Database or ChatRooms table does not exist

Wait for SQL Server to finish starting, then run from Windows:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update
```

## Reference

- [Microsoft: SQL Server Docker quickstart](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?view=sql-server-linux-ver17)
