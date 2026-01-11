namespace LowRollers.Api.Data.Entities;

public class TableTemplate
{
    public Guid TemplateId { get; set; }
    public Guid OwnerId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string ConfigJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedOn { get; set; }

    public Player Owner { get; set; } = null!;
}
