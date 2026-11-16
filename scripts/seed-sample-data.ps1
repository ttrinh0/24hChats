param(
    [string]$Email
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot

# Escape a SQL string literal; the email is never interpreted as SQL.
$emailLiteral = if ([string]::IsNullOrWhiteSpace($Email)) {
    'NULL'
} else {
    "N'" + $Email.Trim().Replace("'", "''") + "'"
}

$sql = @"
SET NOCOUNT ON;
SET XACT_ABORT ON;
DECLARE @email nvarchar(256) = $emailLiteral;
DECLARE @userId nvarchar(450);

IF @email IS NULL
BEGIN
    IF (SELECT COUNT(*) FROM dbo.AspNetUsers) <> 1
        THROW 50000, 'Register an account first, or use -Email to select an existing account.', 1;
    SELECT @userId = Id FROM dbo.AspNetUsers;
END
ELSE
BEGIN
    IF (SELECT COUNT(*) FROM dbo.AspNetUsers WHERE UPPER(Email) = UPPER(@email)) <> 1
        THROW 50001, 'Email must match exactly one existing account.', 1;
    SELECT @userId = Id FROM dbo.AspNetUsers WHERE UPPER(Email) = UPPER(@email);
END

DECLARE @now datetimeoffset = SYSUTCDATETIME();
DECLARE @samples TABLE (Name nvarchar(80), Description nvarchar(160), HoursAgo int, Message nvarchar(max));
INSERT INTO @samples VALUES
    (N'Coffee Chat', N'A casual place to say hello and talk about your day.', 2, N'Hello! What is everyone drinking today?'),
    (N'Music Corner', N'Share songs, albums, and artists you enjoy.', 5, N'What have you been listening to lately?'),
    (N'Game Night', N'Talk about your favourite games and find people to play with.', 9, N'What game should we try next?'),
    (N'Ideas and Projects', N'Share what you are making or learning.', 15, N'What are you working on?');

DECLARE @created TABLE (Id uniqueidentifier, Name nvarchar(80), CreatedAt datetimeoffset);
BEGIN TRANSACTION;
INSERT INTO dbo.ChatRooms (Id, Name, Description, OwnerId, Theme, CreatedAt, ExpiresAt)
    OUTPUT inserted.Id, inserted.Name, inserted.CreatedAt INTO @created
    SELECT NEWID(), source.Name, source.Description, @userId, N'',
           DATEADD(hour, -source.HoursAgo, @now), DATEADD(hour, 24 - source.HoursAgo, @now)
    FROM @samples AS source
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.ChatRooms AS room
        WHERE room.OwnerId = @userId AND room.Name = source.Name AND room.ExpiresAt > @now
    );
INSERT INTO dbo.ChatMembers (UserId, RoomId, JoinedAt)
    SELECT @userId, Id, CreatedAt FROM @created;
INSERT INTO dbo.ChatMessages (Id, RoomId, UserId, Message, Colour, SentAt)
    SELECT NEWID(), created.Id, @userId, sample.Message, N'', created.CreatedAt
    FROM @created AS created JOIN @samples AS sample ON sample.Name = created.Name;
COMMIT TRANSACTION;
SELECT COUNT(*) AS RoomsCreated FROM @created;
"@

Push-Location $projectRoot
try {
    $sql | docker compose exec -T db /bin/bash -c '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -d Chats24h -b'
    if ($LASTEXITCODE -ne 0) {
        throw "Sample data could not be added (sqlcmd exited with code $LASTEXITCODE)."
    }
}
finally {
    Pop-Location
}
