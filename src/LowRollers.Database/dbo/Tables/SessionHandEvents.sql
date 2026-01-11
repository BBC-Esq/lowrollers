CREATE TABLE dbo.SessionHandEvents (
    HandDetailId    UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_SessionHandEvents_Id DEFAULT NEWSEQUENTIALID(),
    SessionId       UNIQUEIDENTIFIER    NOT NULL,
    HandId          UNIQUEIDENTIFIER    NOT NULL,
    PlayerId        UNIQUEIDENTIFIER        NULL,
    EventTypeId     TINYINT             NOT NULL,
    EventTimestamp  DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_SessionHandEvents_Timestamp DEFAULT SYSDATETIMEOFFSET(),
    Amount          INT                     NULL,
    EventDetails    VARCHAR(MAX)            NULL,
    CONSTRAINT PK_SessionHandEvents PRIMARY KEY (HandDetailId),
    CONSTRAINT FK_SessionHandEvents_SessionHands FOREIGN KEY (SessionId, HandId) REFERENCES dbo.SessionHands(SessionId, HandId),
    CONSTRAINT FK_SessionHandEvents_Players FOREIGN KEY (PlayerId) REFERENCES dbo.Players(PlayerId)
);

GO

CREATE NONCLUSTERED INDEX IX_SessionHandEvents_HandId
ON dbo.SessionHandEvents (HandId);

GO

CREATE NONCLUSTERED INDEX IX_SessionHandEvents_SessionId
ON dbo.SessionHandEvents (SessionId);

GO

CREATE NONCLUSTERED INDEX IX_SessionHandEvents_PlayerId
ON dbo.SessionHandEvents (PlayerId)
WHERE PlayerId IS NOT NULL;

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Event sourcing for hand history. EventTypeId maps to: SB, BB, Deal, Flop, Turn, River, Check, Bet, Fold, Winner, etc.
PlayerId is NULL for system events (deals, community cards). EventDetails stores JSON (hole cards, community cards, etc.).',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'SessionHandEvents';
