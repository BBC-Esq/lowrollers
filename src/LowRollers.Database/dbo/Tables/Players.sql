CREATE TABLE dbo.Players (
    PlayerId        UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_Players_PlayerId DEFAULT NEWSEQUENTIALID(),
    CreatedOn       DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_Players_CreatedOn DEFAULT SYSDATETIMEOFFSET(),
    PlayerName      VARCHAR(100)        NOT NULL,
    PlayerEmail     VARCHAR(255)            NULL,
    Avatar          VARBINARY(MAX)          NULL,
    CONSTRAINT PK_Players PRIMARY KEY (PlayerId)
);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Global player registry. Players are created when they first join any table.
Email is optional - can be used for invitations or passwordless login.
A special Dealer player (00000000-0000-0000-0000-000000000000) is used for system events.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Players';
