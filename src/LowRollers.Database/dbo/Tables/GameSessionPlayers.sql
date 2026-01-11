CREATE TABLE dbo.GameSessionPlayers (
    SessionId           UNIQUEIDENTIFIER    NOT NULL,
    PlayerId            UNIQUEIDENTIFIER    NOT NULL,
    SeatedOn            DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_GameSessionPlayers_SeatedOn DEFAULT SYSDATETIMEOFFSET(),
    DepartedOn          DATETIMEOFFSET          NULL,
    SeatNumber          TINYINT             NOT NULL,
    TimeBankSeconds     INT                 NOT NULL    CONSTRAINT DF_GameSessionPlayers_TimeBank DEFAULT 0,
    ChipStack           INT                 NOT NULL    CONSTRAINT DF_GameSessionPlayers_ChipStack DEFAULT 0,
    IsHost              BIT                 NOT NULL    CONSTRAINT DF_GameSessionPlayers_IsHost DEFAULT 0,
    CONSTRAINT PK_GameSessionPlayers PRIMARY KEY (SessionId, PlayerId),
    CONSTRAINT FK_GameSessionPlayers_GameSessions FOREIGN KEY (SessionId) REFERENCES dbo.GameSessions(SessionId),
    CONSTRAINT FK_GameSessionPlayers_Players FOREIGN KEY (PlayerId) REFERENCES dbo.Players(PlayerId),
    CONSTRAINT CHK_GameSessionPlayers_SeatNumber CHECK (SeatNumber BETWEEN 0 AND 10)
);

GO

CREATE NONCLUSTERED INDEX IX_GameSessionPlayers_PlayerId
ON dbo.GameSessionPlayers (PlayerId);

GO

CREATE UNIQUE INDEX UX_GameSessionPlayers_SessionId_SeatNumber
ON dbo.GameSessionPlayers (SessionId, SeatNumber)
WHERE SeatNumber > 0;

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Player-session junction table. SeatNumber 0 is reserved for the Dealer (system).
ChipStack is session-scoped balance. IsHost indicates current host (can transfer).
TimeBankSeconds tracks available time bank for action timer.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'GameSessionPlayers';
