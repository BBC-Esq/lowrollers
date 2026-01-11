CREATE TABLE dbo.TableTemplates (
    TemplateId      UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT DF_TableTemplates_Id DEFAULT NEWSEQUENTIALID(),
    OwnerId         UNIQUEIDENTIFIER    NOT NULL,
    TemplateName    VARCHAR(100)        NOT NULL,
    ConfigJson      VARCHAR(MAX)        NOT NULL,
    CreatedOn       DATETIMEOFFSET      NOT NULL    CONSTRAINT DF_TableTemplates_CreatedOn DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT PK_TableTemplates PRIMARY KEY (TemplateId),
    CONSTRAINT FK_TableTemplates_Players FOREIGN KEY (OwnerId) REFERENCES dbo.Players(PlayerId)
);

GO

CREATE NONCLUSTERED INDEX IX_TableTemplates_OwnerId
ON dbo.TableTemplates (OwnerId);

GO

EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Saved table configurations that users can reuse when creating new tables.
ConfigJson stores the full configuration (blinds, buy-in limits, timers, etc.).',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'TableTemplates';
