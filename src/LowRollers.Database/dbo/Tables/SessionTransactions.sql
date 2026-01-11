CREATE TABLE dbo.SessionTransactions (
    SessionTransactionId    UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_SessionTransactions_Id DEFAULT NEWSEQUENTIALID(),
    SessionId               UNIQUEIDENTIFIER    NOT NULL,
    PlayerId                UNIQUEIDENTIFIER    NOT NULL,
    TransactionDate         DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_SessionTransactions_Date DEFAULT SYSDATETIMEOFFSET(),
    IsCredit                BIT                 NOT NULL,
    Amount                  INT                 NOT NULL,
    CONSTRAINT PK_SessionTransactions PRIMARY KEY (SessionTransactionId),
    CONSTRAINT FK_SessionTransactions_GameSessions FOREIGN KEY (SessionId) REFERENCES dbo.GameSessions(SessionId),
    CONSTRAINT FK_SessionTransactions_Players FOREIGN KEY (PlayerId) REFERENCES dbo.Players(PlayerId),
    CONSTRAINT CHK_SessionTransactions_Amount CHECK (Amount >= 0)
);

GO

CREATE NONCLUSTERED INDEX IX_SessionTransactions_SessionId
ON dbo.SessionTransactions (SessionId);

GO

CREATE NONCLUSTERED INDEX IX_SessionTransactions_PlayerId
ON dbo.SessionTransactions (PlayerId);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Buy-in/rebuy/cash-out tracking for session accounting.
IsCredit: 1 = buy-in/rebuy, 0 = cash-out. Amount is in chip units (whole numbers).
Sum of credits must equal sum of debits when session ends.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'SessionTransactions';
