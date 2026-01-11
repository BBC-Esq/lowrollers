CREATE TABLE dbo.GameSessions (
    SessionId           UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_GameSessions_SessionId DEFAULT NEWSEQUENTIALID(),
    TableId             UNIQUEIDENTIFIER    NOT NULL,
    StartedOn           DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_GameSessions_StartedOn DEFAULT SYSDATETIMEOFFSET(),
    EndedOn             DATETIMEOFFSET          NULL,
    TableConfigOverride VARCHAR(MAX)            NULL,
    InviteCodeHash      VARCHAR(255)        NOT NULL,
    PasswordHash        VARCHAR(255)            NULL,
    CONSTRAINT PK_GameSessions PRIMARY KEY (SessionId),
    CONSTRAINT FK_GameSessions_GameTables FOREIGN KEY (TableId) REFERENCES dbo.GameTables(TableId)
);

GO

CREATE NONCLUSTERED INDEX IX_GameSessions_TableId
ON dbo.GameSessions (TableId);

GO

CREATE NONCLUSTERED INDEX IX_GameSessions_InviteCodeHash
ON dbo.GameSessions (InviteCodeHash);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Active game instances for a table. InviteCodeHash stores a hash of the invite code (not plaintext).
PasswordHash is optional for additional table security. TableConfigOverride allows per-session overrides.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'GameSessions';
