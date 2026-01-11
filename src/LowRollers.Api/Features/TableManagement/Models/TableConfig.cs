namespace LowRollers.Api.Features.TableManagement.Models;

/// <summary>
/// JSON-serializable table configuration stored in GameTables.TableConfig
/// </summary>
public sealed class TableConfig
{
    /// <summary>
    /// Small blind amount (whole dollars only)
    /// </summary>
    public int SmallBlind { get; set; } = 1;

    /// <summary>
    /// Big blind amount (must be 2x small blind)
    /// </summary>
    public int BigBlind { get; set; } = 2;

    /// <summary>
    /// Minimum buy-in amount (whole dollars only)
    /// </summary>
    public int MinBuyIn { get; set; } = 40;

    /// <summary>
    /// Maximum buy-in amount (whole dollars only)
    /// </summary>
    public int MaxBuyIn { get; set; } = 200;

    /// <summary>
    /// Maximum number of seats at the table (2-10)
    /// </summary>
    public int MaxSeats { get; set; } = 9;

    /// <summary>
    /// Action timer in seconds (0 = unlimited)
    /// </summary>
    public int ActionTimerSeconds { get; set; } = 30;

    /// <summary>
    /// Whether time bank is enabled
    /// </summary>
    public bool TimeBankEnabled { get; set; } = true;

    /// <summary>
    /// Time bank duration in seconds
    /// </summary>
    public int TimeBankSeconds { get; set; } = 60;

    /// <summary>
    /// Pause duration between hands in seconds
    /// </summary>
    public int PauseBetweenHandsSeconds { get; set; } = 3;

    /// <summary>
    /// Showdown duration in seconds (0 = manual)
    /// </summary>
    public int ShowdownDurationSeconds { get; set; } = 5;

    /// <summary>
    /// Auto-muck losing hands at showdown
    /// </summary>
    public bool AutoMuckLosers { get; set; } = true;

    /// <summary>
    /// Create a default configuration
    /// </summary>
    public static TableConfig CreateDefault() => new();

    /// <summary>
    /// Create configuration from blind values
    /// </summary>
    public static TableConfig CreateWithBlinds(int smallBlind, int bigBlind)
    {
        return new TableConfig
        {
            SmallBlind = smallBlind,
            BigBlind = bigBlind,
            MinBuyIn = bigBlind * 20,
            MaxBuyIn = bigBlind * 100
        };
    }

    /// <summary>
    /// Validates the table configuration against business rules
    /// </summary>
    /// <returns>A tuple indicating validity and any error messages</returns>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        // Small blind must be positive
        if (SmallBlind <= 0)
            errors.Add("Small blind must be greater than zero");

        // HOST-TABLE-002: Big blind must be 2x small blind
        if (BigBlind != SmallBlind * 2)
            errors.Add("Big blind must be exactly 2x small blind");

        // HOST-TABLE-003: Min buy-in minimum 20x BB, maximum 100x BB
        if (MinBuyIn < BigBlind * 20 || MinBuyIn > BigBlind * 100)
            errors.Add("Minimum buy-in must be between 20x and 100x big blind");

        // HOST-TABLE-004: Max buy-in minimum 100x BB
        if (MaxBuyIn < BigBlind * 100)
            errors.Add("Maximum buy-in must be at least 100x big blind");

        // HOST-TABLE-005: MaxSeats must be 2-10
        if (MaxSeats < 2 || MaxSeats > 10)
            errors.Add("Table must have between 2 and 10 seats");

        return (errors.Count == 0, errors);
    }
}
