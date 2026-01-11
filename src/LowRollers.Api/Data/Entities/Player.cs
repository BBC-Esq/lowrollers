namespace LowRollers.Api.Data.Entities;

public class Player
{
    public Guid PlayerId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? PlayerEmail { get; set; }
    public byte[]? Avatar { get; set; }

    public ICollection<GameTable> CreatedTables { get; set; } = [];
    public ICollection<GameTable> ModifiedTables { get; set; } = [];
    public ICollection<GameTable> OwnedTables { get; set; } = [];
    public ICollection<GameSessionPlayer> GameSessionPlayers { get; set; } = [];
    public ICollection<SessionTransaction> SessionTransactions { get; set; } = [];
    public ICollection<SessionHandEvent> SessionHandEvents { get; set; } = [];
    public ICollection<TableTemplate> TableTemplates { get; set; } = [];
}
