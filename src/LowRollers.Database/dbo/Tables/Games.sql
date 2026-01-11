CREATE TABLE dbo.Games (
    GameId      TINYINT         NOT NULL,
    GameName    VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_Games PRIMARY KEY (GameId)
);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Lookup table for game types (e.g., Texas Holdem)',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Games';
