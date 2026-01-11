namespace LowRollers.Api.Data.Entities;

public class Game
{
    public byte GameId { get; set; }
    public string GameName { get; set; } = string.Empty;

    public ICollection<GameTable> GameTables { get; set; } = [];
}
