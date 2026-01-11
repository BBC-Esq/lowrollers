CREATE TABLE dbo.GameTables (
    TableId         UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_GameTables_TableId DEFAULT NEWSEQUENTIALID(),
    CreatedOn       DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_GameTables_CreatedOn DEFAULT SYSDATETIMEOFFSET(),
    CreatedBy       UNIQUEIDENTIFIER    NOT NULL,
    ModifiedOn      DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_GameTables_ModifiedOn DEFAULT SYSDATETIMEOFFSET(),
    ModifiedBy      UNIQUEIDENTIFIER    NOT NULL,
    TableName       VARCHAR(100)        NOT NULL,
    GameId          TINYINT             NOT NULL,
    TableOwner      UNIQUEIDENTIFIER    NOT NULL,
    TableConfig     VARCHAR(MAX)        NOT NULL,
    CONSTRAINT PK_GameTables PRIMARY KEY (TableId),
    CONSTRAINT FK_GameTables_Games FOREIGN KEY (GameId) REFERENCES dbo.Games(GameId),
    CONSTRAINT FK_GameTables_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Players(PlayerId),
    CONSTRAINT FK_GameTables_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES dbo.Players(PlayerId),
    CONSTRAINT FK_GameTables_TableOwner FOREIGN KEY (TableOwner) REFERENCES dbo.Players(PlayerId)
);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Reusable table templates with configuration (blinds, buy-in limits, timers, etc.).
TableConfig stores settings as JSON. Sessions reference tables for configuration inheritance.',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'GameTables';
