namespace LowRollers.Api.Features.TableManagement.Models;

/// <summary>
/// Table session status
/// </summary>
public enum TableStatus
{
    /// <summary>
    /// Table is in lobby, waiting for players or game to start
    /// </summary>
    Lobby,

    /// <summary>
    /// Game is actively being played
    /// </summary>
    Active,

    /// <summary>
    /// Game is paused between hands
    /// </summary>
    Paused,

    /// <summary>
    /// Session has ended
    /// </summary>
    Closed
}

/// <summary>
/// Response containing the current table state
/// </summary>
public sealed class TableStateResponse
{
    /// <summary>
    /// The table ID
    /// </summary>
    public Guid TableId { get; set; }

    /// <summary>
    /// The session ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// Table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Current table status
    /// </summary>
    public TableStatus Status { get; set; } = TableStatus.Lobby;

    /// <summary>
    /// Table configuration
    /// </summary>
    public TableConfig Config { get; set; } = new();

    /// <summary>
    /// Current players at the table
    /// </summary>
    public List<TablePlayerInfo> Players { get; set; } = [];

    /// <summary>
    /// The host's player ID
    /// </summary>
    public Guid HostPlayerId { get; set; }

    /// <summary>
    /// Whether the game is currently in progress
    /// </summary>
    public bool GameInProgress { get; set; }
}

/// <summary>
/// Player information for table state
/// </summary>
public sealed class TablePlayerInfo
{
    /// <summary>
    /// Player ID
    /// </summary>
    public Guid PlayerId { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Seat number (0 = standing/lobby)
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Current chip stack
    /// </summary>
    public int ChipStack { get; set; }

    /// <summary>
    /// Whether this player is the host
    /// </summary>
    public bool IsHost { get; set; }

    /// <summary>
    /// Whether the player is currently connected
    /// </summary>
    public bool IsConnected { get; set; }
}
