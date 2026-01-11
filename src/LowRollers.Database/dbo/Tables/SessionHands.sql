CREATE TABLE dbo.SessionHands (
    HandId          UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_SessionHands_HandId DEFAULT NEWSEQUENTIALID(),
    SessionId       UNIQUEIDENTIFIER    NOT NULL,
    ShuffleSeed     VARBINARY(255)      NOT NULL,
    StartedOn       DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_SessionHands_StartedOn DEFAULT SYSDATETIMEOFFSET(),
    EndedOn         DATETIMEOFFSET          NULL,
    CONSTRAINT PK_SessionHands PRIMARY KEY (HandId),
    CONSTRAINT FK_SessionHands_GameSessions FOREIGN KEY (SessionId) REFERENCES dbo.GameSessions(SessionId)
);

GO

CREATE UNIQUE NONCLUSTERED INDEX IX_SessionHands_SessionId_HandId
ON dbo.SessionHands (SessionId, HandId);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Container for each hand played. ShuffleSeed stores the cryptographic seed for reproducibility.
Winners and outcomes are tracked in SessionHandEvents (pots can chop).',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'SessionHands';
