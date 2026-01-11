namespace LowRollers.Api.Data.Entities;

public class GameTable
{
    public Guid TableId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTimeOffset ModifiedOn { get; set; }
    public Guid ModifiedBy { get; set; }
    public string TableName { get; set; } = string.Empty;
    public byte GameId { get; set; }
    public Guid TableOwner { get; set; }
    public string TableConfig { get; set; } = string.Empty;

    public Game Game { get; set; } = null!;
    public Player CreatedByPlayer { get; set; } = null!;
    public Player ModifiedByPlayer { get; set; } = null!;
    public Player Owner { get; set; } = null!;
    public ICollection<GameSession> GameSessions { get; set; } = [];
}
